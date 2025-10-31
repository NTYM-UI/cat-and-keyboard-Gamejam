using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Boss虚弱状态提示对话框
/// 直接实现写死的对话框功能，不依赖于事件系统和对话表格
/// </summary>
public class BossWeakStateDialog : MonoBehaviour
{
    [Header("对话框设置")]
    [SerializeField] private GameObject dialogPanel; // 对话框面板
    [SerializeField] private TextMeshProUGUI dialogText; // 对话框文本
    [SerializeField] private string weakStateMessage = "鼠标点击对BOSS造成伤害";
    [SerializeField] private float dialogDuration = 3f; // 对话框显示持续时间
    
    [Header("淡入淡出设置")]
    [SerializeField] private float fadeInDuration = 0.3f; // 淡入时间
    [SerializeField] private float fadeOutDuration = 0.3f; // 淡出时间
    
    private CanvasGroup canvasGroup; // 用于控制淡入淡出效果
    private float dialogStartTime; // 对话框开始显示的时间
    private bool isDialogActive = false; // 对话框是否激活
    
    private void Awake()
    {
        // 获取CanvasGroup组件用于淡入淡出效果
        canvasGroup = dialogPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dialogPanel.AddComponent<CanvasGroup>();
        }
        
        // 初始化对话框状态
        dialogPanel.SetActive(false);
        canvasGroup.alpha = 0f;
        
        // 设置对话框文本
        if (dialogText != null)
        {
            dialogText.text = weakStateMessage;
        }
    }
    
    private void OnEnable()
    {
        // 订阅Boss虚弱状态相关事件
        BossAI.OnBossEnterWeakState += ShowDialog;
        BossAI.OnBossExitWeakState += HideDialog;
    }
    
    private void OnDisable()
    {
        // 取消订阅事件
        BossAI.OnBossEnterWeakState -= ShowDialog;
        BossAI.OnBossExitWeakState -= HideDialog;
    }
    
    private void Update()
    {
        if (isDialogActive)
        {
            // 处理淡入效果
            if (Time.time < dialogStartTime + fadeInDuration)
            {
                float t = (Time.time - dialogStartTime) / fadeInDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }
            // 处理淡出效果
            else if (Time.time >= dialogStartTime + dialogDuration)
            {
                float t = (Time.time - dialogStartTime - dialogDuration) / fadeOutDuration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                
                if (t >= 1f)
                {
                    dialogPanel.SetActive(false);
                    isDialogActive = false;
                }
            }
        }
    }
    
    /// <summary>
    /// 显示对话框
    /// </summary>
    private void ShowDialog()
    {
        dialogPanel.SetActive(true);
        dialogStartTime = Time.time;
        isDialogActive = true;
        canvasGroup.alpha = 0f; // 重置透明度，准备淡入
    }
    
    /// <summary>
    /// 隐藏对话框
    /// </summary>
    private void HideDialog()
    {
        // 立即开始淡出
        StartCoroutine(FadeOutImmediate());
    }
    
    /// <summary>
    /// 立即淡出对话框
    /// </summary>
    private System.Collections.IEnumerator FadeOutImmediate()
    {
        float startAlpha = canvasGroup.alpha;
        float startTime = Time.time;
        
        while (Time.time < startTime + fadeOutDuration)
        {
            float t = (Time.time - startTime) / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        dialogPanel.SetActive(false);
        isDialogActive = false;
        canvasGroup.alpha = 0f;
    }
}