using UnityEngine;
/// <summary>
/// Boss控制器
/// 管理Boss的初始状态和事件监听
/// 当对话ID为110的对话结束后，Boss将被逐渐激活并显示特效
/// </summary>
public class BossController : MonoBehaviour
{
    [Tooltip("要监听的对话结束事件ID")]
    [SerializeField] private int targetDialogId = 110;
    
    [Tooltip("Boss的主要模型或需要激活的子对象")]
    [SerializeField] private GameObject bossModel;
    
    [Tooltip("Boss出现时的特效")]
    [SerializeField] private GameObject appearanceEffect;
    
    [Tooltip("Boss出现动画持续时间（秒）")]
    [SerializeField] private float appearanceDuration = 1.5f;
    
    [Tooltip("是否启用淡入效果")]
    [SerializeField] private bool enableFadeIn = true;
    
    // 存储原始缩放值
    private Vector3 originalScale;
    
    // 存储所有需要淡入的Renderer组件
    private Renderer[] renderers;
    
    // 标记Boss是否已经被激活
    private bool isBossActivated = false;
    
    private void Awake()
    {
        // 获取所有需要淡入的Renderer组件
        renderers = GetComponentsInChildren<Renderer>();
        
        // 保存原始缩放值
        originalScale = transform.localScale;
        
        // 初始化Boss状态为失活
        SetBossActive(false);
    }
    
    private void OnEnable()
    {
        // 订阅对话结束事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
        }
    }
    
    private void OnDisable()
    {
        // 取消订阅对话结束事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
        }
    }
    
    /// <summary>
    /// 处理对话结束事件
    /// 当对话ID为目标ID时激活Boss
    /// </summary>
    private void OnDialogEnd(object data)
    {
        if (data is int dialogId && dialogId == targetDialogId && !isBossActivated)
        {
            ActivateBoss();
        }
    }
    
    /// <summary>
    /// 激活Boss
    /// </summary>
    private void ActivateBoss()
    {
        // 标记Boss为已激活
        isBossActivated = true;
        
        // 立即激活Boss对象，但不显示（通过协程逐渐显示）
        if (bossModel != null)
        {
            bossModel.SetActive(true);
        }        
        else
        {
            gameObject.SetActive(true);
        }
        
        // 播放出现特效
        if (appearanceEffect != null)
        {
            // 使用bossModel的位置（如果有），否则使用当前对象的位置
            Vector3 effectPosition = bossModel != null ? bossModel.transform.position : transform.position;
            GameObject effect = Instantiate(appearanceEffect, effectPosition, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // 开始逐渐出现的协程
        StartCoroutine(AnimateBossAppearance());
    }
    
    /// <summary>
    /// 协程：实现Boss逐渐出现的动画
    /// </summary>
    private System.Collections.IEnumerator AnimateBossAppearance()
    {
        float elapsedTime = 0f;
        
        // 获取需要操作的目标对象和渲染器
        GameObject targetObject = bossModel != null ? bossModel : gameObject;
        Renderer[] targetRenderers = bossModel != null 
            ? bossModel.GetComponentsInChildren<Renderer>() 
            : renderers;
        
        // 初始化透明度
        if (enableFadeIn && targetRenderers.Length > 0)
        {
            foreach (Renderer renderer in targetRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = 0f;
                        material.color = color;
                    }
                }
            }
        }
        
        // 动画循环
        while (elapsedTime < appearanceDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / appearanceDuration);
            
            // 使用平滑曲线计算进度
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // 更新透明度
            if (enableFadeIn && targetRenderers.Length > 0)
            {
                foreach (Renderer renderer in targetRenderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        if (material.HasProperty("_Color"))
                        {
                            Color color = material.color;
                            color.a = smoothT;
                            material.color = color;
                        }
                    }
                }
            }
            
            yield return null;
        }
        
        // 确保最终状态正确
        targetObject.transform.localScale = originalScale;
        
        if (enableFadeIn && targetRenderers.Length > 0)
        {
            foreach (Renderer renderer in targetRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = 1f;
                        material.color = color;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 设置Boss的激活状态
    /// </summary>
    /// <param name="active">是否激活</param>
    private void SetBossActive(bool active)
    {
        // 如果指定了bossModel，则只激活/失活该模型
        if (bossModel != null)
        {
            if (active)
            {
                bossModel.SetActive(true);
                
                // 如果启用淡入效果，设置初始透明度为0
                if (enableFadeIn)
                {
                    Renderer[] bossRenderers = bossModel.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in bossRenderers)
                    {
                        foreach (Material material in renderer.materials)
                        {
                            if (material.HasProperty("_Color"))
                            {
                                Color color = material.color;
                                color.a = 0f;
                                material.color = color;
                            }
                        }
                    }
                }
            }
            else
            {
                bossModel.SetActive(false);
            }
        }
        else
        {
            // 否则激活/失活整个游戏对象
            if (active)
            {
                gameObject.SetActive(true);
                
                // 如果启用淡入效果，设置初始透明度为0
                if (enableFadeIn && renderers.Length > 0)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material material in renderer.materials)
                        {
                            if (material.HasProperty("_Color"))
                            {
                                Color color = material.color;
                                color.a = 0f;
                                material.color = color;
                            }
                        }
                    }
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 手动激活Boss（用于测试或其他脚本调用）
    /// </summary>
    public void ManualActivateBoss()
    {
        if (!isBossActivated)
        {
            ActivateBoss();
        }
    }
}