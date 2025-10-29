using UnityEngine;

/// <summary>
/// Boss死亡序列控制脚本
/// 处理Boss死亡后的对话触发、传送门显示和游戏结束流程
/// </summary>
public class BossDeathSequence : MonoBehaviour
{
    [Header("对话设置")]
    [SerializeField] private int bossDeathDialogId = 111; // Boss死亡后要显示的对话ID
    [SerializeField] private int portalPrefabId = 113; // 传送门显示的对话ID

    [Header("传送门设置")]
    [SerializeField] private GameObject portalPrefab; // 传送门预制体

    private void Start()
    {
        portalPrefab.SetActive(false);
    }

    private void OnEnable()
    {
        // 订阅Boss死亡事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe(GameEventNames.BOSS_DEFEATED, OnBossDefeated);
            EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
        }
    }
    
    private void OnDisable()
    {
        // 取消订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.BOSS_DEFEATED, OnBossDefeated);
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
        }
    }
    
    /// <summary>
    /// 处理Boss死亡事件
    /// </summary>
    private void OnBossDefeated(object data)
    {
        Debug.Log("检测到Boss死亡，触发死亡对话");
        // 触发Boss死亡对话
        if (EventManager.Instance != null)
        {
            // 游戏中使用DIALOG_START事件来触发指定ID的对话
            EventManager.Instance.Publish(GameEventNames.DIALOG_START, bossDeathDialogId);
        }
    }
    
    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    private void OnDialogEnd(object data)
    {
        // 检查是否是Boss死亡对话结束
        if (data is int dialogId && dialogId == portalPrefabId)
        {
            Debug.Log("Boss死亡对话结束，显示传送门");
            // 激活传送门
            if (portalPrefab != null)
            {
                portalPrefab.SetActive(true);
                Debug.Log("显示传送门");
            }
        }
    }
}