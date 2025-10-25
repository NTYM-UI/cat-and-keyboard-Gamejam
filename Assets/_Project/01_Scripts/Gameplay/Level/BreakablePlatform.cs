using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 可断裂的平台
/// 当玩家踩上去或满足特定条件时，平台会断裂并产生物理效果
/// </summary>
public class BreakablePlatform : MonoBehaviour
{
    [Header("断裂设置")]
    [Tooltip("玩家踩上后多久开始断裂")]
    [Range(0.1f, 2f)]
    public float breakDelay = 0.5f;
    
    [Tooltip("断裂时是否触发相机震动")]
    public bool triggerCameraShake = true;
    
    [Header("震动参数")]
    [Tooltip("震动持续时间")]
    [Range(0.1f, 2f)]
    public float shakeDuration = 0.5f;
    
    [Tooltip("震动幅度")]
    [Range(0.05f, 1f)]
    public float shakeMagnitude = 0.2f;
    
    [Header("物理设置")]
    [Tooltip("断裂后是否添加物理效果")]
    public bool addPhysicsOnBreak = true;
    
    [Tooltip("断裂后的物理力度")]
    [Range(0f, 10f)]
    public float explosionForce = 5f;
    
    [Tooltip("断裂后多久销毁平台")]
    [Range(1f, 10f)]
    public float destroyAfter = 3f;
    
    [Header("对话触发设置")]
    [Tooltip("触发断裂的对话结束ID")]
    public int targetDialogEndId = 55;
    [Tooltip("是否禁用玩家碰撞触发")]
    public bool disableCollisionTrigger = true;
    
    private Collider2D platformCollider;
    private bool isBreaking = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    private void Awake()
    {
        // 获取组件引用
        platformCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // 确保平台初始时是静态的
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
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
    /// 当玩家踩上平台时触发断裂
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 只有当未禁用碰撞触发时才执行
        if (!disableCollisionTrigger && collision.gameObject.CompareTag("Player") && !isBreaking)
        {
            StartCoroutine(BreakSequence());
        }
    }
    
    /// <summary>
    /// 处理对话结束事件，当对话ID为目标ID时触发断裂
    /// </summary>
    private void OnDialogEnd(object data)
    {
        if (data is int dialogId && dialogId == targetDialogEndId && !isBreaking)
        {
            Debug.Log($"检测到对话结束ID {dialogId}，触发平台断裂");
            StartCoroutine(BreakSequence());
        }
    }
    
    /// <summary>
    /// 断裂序列协程
    /// </summary>
    private IEnumerator BreakSequence()
    {
        isBreaking = true;
        
        // 触发相机震动
        if (triggerCameraShake)
        {
            TriggerShake();
        }
        
        // 触发断裂动画（如果有）
        if (animator != null)
        {
            animator.SetTrigger("Break");
        }
        
        // 发布平台断裂事件
        EventManager.Instance.Publish(GameEventNames.PLATFORM_BREAK, gameObject);
        
        // 移除碰撞器，让玩家掉下去
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }
        
        // 添加物理效果
        if (addPhysicsOnBreak && rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            // 添加一些随机力使断裂看起来更自然
            Vector2 randomForce = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)) * explosionForce;
            rb.AddForce(randomForce, ForceMode2D.Impulse);
            // 添加一些旋转
            rb.AddTorque(Random.Range(-5f, 5f));
        }
        
        // 淡出效果
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOut());
        }
        
        // 延迟后销毁平台
        yield return new WaitForSeconds(destroyAfter);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 触发相机震动
    /// </summary>
    private void TriggerShake()
    {
        if (EventManager.Instance != null)
        {
            // 创建震动参数字典
            Dictionary<string, object> shakeData = new Dictionary<string, object>
            {
                { "duration", shakeDuration },  // 震动持续时间
                { "magnitude", shakeMagnitude } // 震动幅度
            };
            
            // 发布相机震动事件
            EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, shakeData);
            Debug.Log($"平台断裂触发相机震动: 持续时间={shakeDuration}, 幅度={shakeMagnitude}");
        }
    }
    
    /// <summary>
    /// 淡出效果协程
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsed = 0f;
        
        while (elapsed < destroyAfter)
        {
            Color newColor = spriteRenderer.color;
            newColor.a = Mathf.Lerp(startAlpha, 0, elapsed / destroyAfter);
            spriteRenderer.color = newColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }
    
    /// <summary>
    /// 手动触发平台断裂（可通过其他脚本调用）
    /// </summary>
    public void ForceBreak()
    {
        if (!isBreaking)
        {
            StartCoroutine(BreakSequence());
        }
    }
    
    /// <summary>
    /// 通过事件触发断裂
    /// 可以在GameEventNames中添加对应的事件名称
    /// </summary>
    public void BreakByEvent(object data)
    {
        // 可以根据data参数添加额外的条件判断
        ForceBreak();
    }
}