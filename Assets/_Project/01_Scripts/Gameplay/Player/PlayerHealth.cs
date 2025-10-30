using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField] private int maxHealth = 5; // 最大生命值
    private int currentHealth; // 当前生命值

    [Header("无敌设置")]
    [SerializeField] private float invincibilityTime = 1.5f; // 无敌时间（秒）
    [SerializeField] private float flashInterval = 0.1f; // 闪烁间隔时间（秒）
    private bool isInvincible = false; // 是否处于无敌状态
    private SpriteRenderer spriteRenderer; // 角色的SpriteRenderer组件

    private void Awake()
    {
        InitializeHealth();
        // 获取SpriteRenderer组件引用
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("未找到SpriteRenderer组件，请确保玩家对象上已添加此组件");
        }
    }

    private void OnEnable()
    {
        // 订阅相关事件
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_RESPAWN, OnPlayerRespawn);
    }

    private void OnDisable()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_RESPAWN, OnPlayerRespawn);
    }

    /// <summary>
    /// 初始化生命值
    /// </summary>
    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        // 发布生命值初始化事件，通知UI更新
        EventManager.Instance.Publish(GameEventNames.HEALTH_UI_INIT, maxHealth);
        // 发布生命值变化事件
        EventManager.Instance.Publish(GameEventNames.HEALTH_CHANGED, currentHealth);
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage = 1)
    {
        // 如果处于无敌状态或生命值已为0，则不受到伤害
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 发布生命值变化事件
        EventManager.Instance.Publish(GameEventNames.HEALTH_CHANGED, currentHealth);

        // 发布玩家受伤音效事件
        EventManager.Instance.Publish(GameEventNames.PLAY_PLAYER_DEATH_SOUND);

        // 只有在生命值大于0时才启动无敌状态
        if (currentHealth > 0)
        {
            StartCoroutine(StartInvincibility());
        }
        // 如果生命值为0，发布死亡事件和死亡音效事件
        else
        {            
            // 发布玩家死亡音效事件
            EventManager.Instance.Publish(GameEventNames.PLAY_PLAYER_DEATH_SOUND);
            // 发布玩家死亡事件
            EventManager.Instance.Publish(GameEventNames.PLAYER_DEATH);
            // 死亡关闭boss战音乐
            EventManager.Instance.Publish(GameEventNames.BOSS_DEFEATED);
        }
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    /// <param name="healAmount">恢复值</param>
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 发布生命值变化事件
        EventManager.Instance.Publish(GameEventNames.HEALTH_CHANGED, currentHealth);
    }

    /// <summary>
    /// 重置生命值
    /// </summary>
    public void ResetHealth()
    {
        InitializeHealth();
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    /// <returns>当前生命值</returns>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 获取最大生命值
    /// </summary>
    /// <returns>最大生命值</returns>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 玩家复活时的回调
    /// </summary>
    private void OnPlayerRespawn(object data)
    {
        ResetHealth();
        // 确保复活后不处于无敌状态
        isInvincible = false;
        // 确保角色完全可见
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// 启动无敌状态和闪烁效果
    /// </summary>
    private IEnumerator StartInvincibility()
    {
        // 设置为无敌状态
        isInvincible = true;
        
        // 如果有SpriteRenderer组件，则启动闪烁效果
        if (spriteRenderer != null)
        {
            float elapsedTime = 0f;
            bool isVisible = true;
            Color originalColor = spriteRenderer.color;
            
            // 在无敌时间内闪烁
            while (elapsedTime < invincibilityTime)
            {
                // 切换可见性
                isVisible = !isVisible;
                
                // 设置透明度
                Color color = spriteRenderer.color;
                color.a = isVisible ? 0.5f : 1f; // 半透明和完全不透明之间切换
                spriteRenderer.color = color;
                
                // 等待闪烁间隔时间
                yield return new WaitForSeconds(flashInterval);
                elapsedTime += flashInterval;
            }
            
            // 恢复原始透明度
            originalColor.a = 1f;
            spriteRenderer.color = originalColor;
        }
        else
        {
            // 如果没有SpriteRenderer组件，只等待无敌时间
            yield return new WaitForSeconds(invincibilityTime);
        }
        
        // 结束无敌状态
        isInvincible = false;
    }
}