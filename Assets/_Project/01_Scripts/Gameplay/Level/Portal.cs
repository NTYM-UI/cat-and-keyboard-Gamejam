using UnityEngine;

/// <summary>场景切换</summary>
public class Portal : MonoBehaviour
{
    [Tooltip("目标场景名称")]
    [SerializeField] private string targetSceneName; // 默认目标场景
    
    private bool isTeleporting = false; // 防止重复触发
    private bool waitingForFadeOut = false; // 是否等待淡出完成

    private void OnEnable()
    {
        // 订阅淡出完成事件
        EventManager.Instance.Subscribe(GameEventNames.FADE_OUT_COMPLETE, OnFadeOutComplete);
    }

    private void OnDisable()
    {
        // 取消订阅淡出完成事件
        EventManager.Instance.Unsubscribe(GameEventNames.FADE_OUT_COMPLETE, OnFadeOutComplete);
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Player") && !isTeleporting && !string.IsNullOrEmpty(targetSceneName))
        {
            isTeleporting = true;
            waitingForFadeOut = true;

            // 触发淡出效果
            EventManager.Instance.Publish(GameEventNames.FADE_OUT_START);
        }
    }

    /// <summary>处理淡出完成事件</summary>
    private void OnFadeOutComplete(object data)
    {
        // 只有在等待淡出时才执行场景加载
        if (waitingForFadeOut && isTeleporting)
        {
            // 淡出完成后加载场景
            SceneManager.Instance.LoadScene(targetSceneName);
            
            // 重置传送状态
            ResetTeleportState();
        }
    }
    
    private void ResetTeleportState()
    {
        isTeleporting = false;
        waitingForFadeOut = false;
    }
}
