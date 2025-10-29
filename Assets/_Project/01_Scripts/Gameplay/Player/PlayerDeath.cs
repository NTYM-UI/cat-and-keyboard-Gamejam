using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.Respawn;

public class PlayerDeath : MonoBehaviour
{
    private int playerDeathCount = 0;
    public int level = 0;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        // 获取PlayerHealth组件
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth组件未找到，请确保已添加到玩家对象上");
        }
    }

    private void OnEnable()
    {
        // 订阅玩家死亡事件
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_DEATH, OnPlayerDeath);
    }

    private void OnDisable()
    {
        // 取消订阅玩家死亡事件
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_DEATH, OnPlayerDeath);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            playerDeathCount++;

            if (level == 1)
            {
                if (playerDeathCount == 1)
                {
                    EventManager.Instance.Publish(GameEventNames.DIALOG_START, 23);
                }

                if (playerDeathCount == 2)
                {
                    EventManager.Instance.Publish(GameEventNames.DIALOG_START, 26);
                }

                if (playerDeathCount > 2)
                {
                    EventManager.Instance.Publish(GameEventNames.DIALOG_START, 28);
                }
            }

            // 调用PlayerHealth的TakeDamage方法
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            else
            {
                // 如果没有PlayerHealth组件，保留原有逻辑作为备用
                Debug.LogWarning("使用备用死亡处理逻辑");
                EventManager.Instance.Publish(GameEventNames.PLAYER_DEATH);
                EventManager.Instance.Publish(GameEventNames.PLAYER_RESPAWN);
            }
        }
    }

    /// <summary>
    /// 玩家死亡事件处理
    /// </summary>
    private void OnPlayerDeath(object data)
    {
        // 延迟发布复活事件，给死亡动画等留出时间
        StartCoroutine(RespawnAfterDelay());
    }

    /// <summary>
    /// 延迟后处理玩家死亡
    /// </summary>
    private IEnumerator RespawnAfterDelay()
    {
        // 可以在这里添加死亡动画时间
        yield return new WaitForSeconds(0.5f);
        
        // 检查当前场景是否为LevelScenes4_1 - 使用兼容的方法获取场景名称
        string currentSceneName = Application.loadedLevelName;
        if (currentSceneName == "LevelScenes4_1")
        {
            // 重新加载当前场景 - 使用兼容的方法加载场景
            Debug.Log("玩家死亡，重新加载LevelScenes4_1场景");
            Application.LoadLevel(currentSceneName);
        }
        else
        {
            // 其他场景保持原有的复活事件发布逻辑
            Debug.Log("玩家死亡，发布复活事件");
            EventManager.Instance.Publish(GameEventNames.PLAYER_RESPAWN);
        }
    }
}
