using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// 隐藏路按钮控制器 - 玩家踩上按钮时显示隐藏路
/// 增加了面积检测功能：只有当玩家面积大于按钮目标面积时才能触发
/// </summary>
public class HiddenPathButton : MonoBehaviour 
{
    [Header("隐藏路设置")]
    public GameObject hiddenPath;           // 要显示的隐藏路对象
    public bool useActivationAnimation = true;  // 按钮激活动画效果
    [Range(0.1f, 0.5f)]
    public float buttonPressDistance = 0.2f;    // 按钮按下垂直位移
    [Range(0.1f, 2f)]
    public float buttonReleaseDelay = 0.2f;     // 按钮弹起时间延迟
    public bool useFadeInEffect = true;         // 隐藏路淡入效果
    [Range(0.1f, 2f)]
    public float fadeInDuration = 0.5f;         // 淡入持续时间
    public bool triggerOnce = true;             // 是否只触发一次
    
    [Header("面积检测设置")]
    [SerializeField] private float playerArea;  // 玩家当前面积
    public float buttonTargetArea = 1f;         // 按钮目标面积（玩家面积需要大于此值才能触发）
    
    [Header("面积显示设置")]
    public bool showAreaDisplay = true;         // 是否显示面积信息
    public TextMeshProUGUI areaDisplayText;     // 显示面积的文本组件
    
    [Header("视觉反馈")]
    public bool triggerCameraShake = false;     // 是否触发相机震动
    [Range(0.1f, 1f)]
    public float shakeDuration = 0.2f;          // 震动持续时间
    [Range(0.05f, 0.5f)]
    public float shakeMagnitude = 0.1f;         // 震动幅度
    
    // 私有变量
    private bool isActivated = false;
    private bool isPressed = false;
    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Coroutine releaseButtonCoroutine = null;
    
    private void Awake()
    {
        // 初始化位置和组件
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // 初始化隐藏路状态
        if (hiddenPath != null)
        {
            hiddenPath.SetActive(false);
            
            // 设置初始透明度
            if (useFadeInEffect)
            {
                foreach (SpriteRenderer renderer in hiddenPath.GetComponentsInChildren<SpriteRenderer>())
                {
                    Color color = renderer.color;
                    color.a = 0f;
                    renderer.color = color;
                }
            }
        }
        
        //// 初始化面积显示
        //if (showAreaDisplay && areaDisplayText == null)
        //{
        //    Debug.LogWarning("面积显示已启用，但未指定TextMeshProUGUI组件！");
        //}
        //else if (showAreaDisplay && areaDisplayText != null)
        //{
        //    UpdateAreaDisplay();
        //}
    }
    
    private void OnEnable()
    {
        // 订阅窗口缩放变化事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe(GameEventNames.WINDOW_SCALE_CHANGED, OnWindowScaleChanged);
        }
    }
    
    private void OnDisable() 
    {
        // 取消订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.WINDOW_SCALE_CHANGED, OnWindowScaleChanged);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 玩家踩踏时的处理
        if (collision.gameObject.CompareTag("Player") && !isPressed)
        {
            // 检查玩家面积条件
            if (CheckAreaCondition())
            {
                // 显示按钮按下效果
                PressButton();

                // 播放音效
                EventManager.Instance.Publish(GameEventNames.PLAY_BOMB_SOUND);

                // 发布按钮按下事件
                EventManager.Instance.Publish(GameEventNames.BOSS_WEAPON_BUTTON_PRESSED);

                // 首次激活或非一次性触发时显示隐藏路
                if (!isActivated || !triggerOnce)
                {
                    ActivateHiddenPath();
                }
            }
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 玩家持续踩踏时，检查面积变化
        if (collision.gameObject.CompareTag("Player") && !isPressed)
        {
            // 检查玩家面积条件
            if (CheckAreaCondition())
            {
                // 显示按钮按下效果
                PressButton();
                
                // 首次激活或非一次性触发时显示隐藏路
                if (!isActivated || !triggerOnce)
                {
                    ActivateHiddenPath();
                }
            }
        }
    }
    
    /// <summary>
    /// 按钮按下效果
    /// </summary>
    private void PressButton()
    {
        if (isPressed) return;
        
        isPressed = true;
        transform.position = originalPosition - new Vector3(0, buttonPressDistance, 0);
        
        // 触发动画
        if (animator != null && useActivationAnimation)
        {
            animator.SetTrigger("Activate");
        }
    }
    
    /// <summary>
    /// 激活隐藏路功能
    /// </summary>
    private void ActivateHiddenPath()
    {
        // 触发相机震动
        if (triggerCameraShake && EventManager.Instance != null)
        {
            System.Collections.Generic.Dictionary<string, object> shakeData = new System.Collections.Generic.Dictionary<string, object>
            {
                { "duration", shakeDuration },
                { "magnitude", shakeMagnitude }
            };
            EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, shakeData);
        }
        
        // 显示隐藏路
        ShowHiddenPath();
        
        // 发布激活事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Publish(GameEventNames.HIDDEN_PATH_ACTIVATED, gameObject);
        }
        
        isActivated = true;
    }
    
    /// <summary>
    /// 显示隐藏路
    /// </summary>
    private void ShowHiddenPath()
    {
        if (hiddenPath == null) return;
        
        hiddenPath.SetActive(true);
        
        if (useFadeInEffect)
        {
            StartCoroutine(FadeInPath());
        }
        else
        {
            // 立即显示
            foreach (SpriteRenderer renderer in hiddenPath.GetComponentsInChildren<SpriteRenderer>())
            {
                Color color = renderer.color;
                color.a = 1f;
                renderer.color = color;
            }
        }
    }
    
    /// <summary>
    /// 隐藏路淡入效果协程
    /// </summary>
    private IEnumerator FadeInPath()
    {
        if (hiddenPath == null) yield break;
        
        SpriteRenderer[] pathRenderers = hiddenPath.GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            
            foreach (SpriteRenderer renderer in pathRenderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保完全不透明
        foreach (SpriteRenderer renderer in pathRenderers)
        {
            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;
        }
    }
    
    /// <summary>
    /// 玩家离开按钮时的处理
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 管理协程避免重复执行
            if (releaseButtonCoroutine != null)
            {
                StopCoroutine(releaseButtonCoroutine);
            }
            releaseButtonCoroutine = StartCoroutine(ReleaseButtonAfterDelay());
        }
    }
    
    /// <summary>
    /// 检查玩家面积是否满足触发条件
    /// </summary>
    /// <returns>如果玩家面积大于目标面积则返回true</returns>
    private bool CheckAreaCondition()
    {
        return playerArea > buttonTargetArea;
    }
    
    /// <summary>
    /// 处理窗口缩放变化事件
    /// </summary>
    /// <param name="data">包含新缩放比例的object数据</param>
    private void Update()
    {
        // 实时更新面积显示
        UpdateAreaDisplay();
    }
    
    private void OnWindowScaleChanged(object data)
    {
        // 将object类型参数转换为Vector3
        if (data is Vector3 newScale)
        {
            playerArea = newScale.x * newScale.y;
            // 面积变化时立即更新显示
            UpdateAreaDisplay();
        }
    }
    
    /// <summary>
    /// 更新面积显示文本
    /// </summary>
    private void UpdateAreaDisplay()
    {
        if (!showAreaDisplay || areaDisplayText == null) return;
        
        // 更新文本内容
        areaDisplayText.text = $"玩家面积: {playerArea:F2}\n目标面积: {buttonTargetArea:F2}";
    }
    
    /// <summary>
    /// 延迟释放按钮的协程
    /// </summary>
    private IEnumerator ReleaseButtonAfterDelay()
    {
        // 使用配置的延迟时间
        yield return new WaitForSeconds(buttonReleaseDelay);
        
        // 检测玩家是否真的离开了
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.8f);
        bool hasPlayerNearby = false;
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                hasPlayerNearby = true;
                break;
            }
        }
        
        // 确认无玩家时恢复按钮
        if (!hasPlayerNearby && isPressed)
        {
            transform.position = originalPosition;
            
            // 非一次性触发时重置状态
            if (!triggerOnce)
            {
                isActivated = false;
            }
            
            isPressed = false;
        }
        
        releaseButtonCoroutine = null;
    }
}