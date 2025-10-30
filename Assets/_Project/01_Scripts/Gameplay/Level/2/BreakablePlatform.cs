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
    
    [Tooltip("断裂后多久开始恢复平台")]
    [Range(1f, 10f)]
    public float recoverAfter = 0.5f;
    
    [Tooltip("是否可以恢复（用于Boss阶段转换时的平台效果）")]
    public bool canRecover = false;
    
    [Header("对话触发设置")]
    [Tooltip("触发断裂的对话结束ID")]
    public int targetDialogEndId = 55;
    [Tooltip("是否禁用玩家碰撞触发")]
    public bool disableCollisionTrigger = true;
    [Header("关卡设置")]
    [Tooltip("是否为Boss关（Boss关不需要对话触发，直接通过特效触发）")]
    public bool isBossLevel = false;
    
    private Collider2D platformCollider;
    private bool isBreaking = false;
    private bool isRecovering = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Vector3 originalPosition; // 存储原始位置
    
    private void Awake()
    {
        // 获取组件引用
        platformCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // 存储原始位置
        originalPosition = transform.position;
        
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
            EventManager.Instance.Subscribe(GameEventNames.BOSS_PHASE_CHANGE, OnBossPhaseChange);
            EventManager.Instance.Subscribe(GameEventNames.PLATFORM_BREAK, OnGroundBreak);
        }
    }
    
    private void OnDisable()
    {
        // 取消订阅对话结束事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
            EventManager.Instance.Unsubscribe(GameEventNames.BOSS_PHASE_CHANGE, OnBossPhaseChange);
            EventManager.Instance.Unsubscribe(GameEventNames.PLATFORM_BREAK, OnGroundBreak);
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
    /// 在非Boss关通过对话触发，Boss关不使用对话触发
    /// </summary>
    private void OnDialogEnd(object data)
    {
        // 只有非Boss关才响应对话结束事件
        if (!isBossLevel && data is int dialogId && dialogId == targetDialogEndId && !isBreaking)
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

        // 发布断裂音效
        EventManager.Instance.Publish(GameEventNames.PLAY_GROUND_CRACKED_SOUND);

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
        
        // 根据是否可恢复决定后续操作
        if (canRecover)
        {
            // 延迟后开始恢复
            yield return new WaitForSeconds(recoverAfter);
            StartCoroutine(RecoverSequence());
        }
        else
        {
            // 延迟后销毁平台
            yield return new WaitForSeconds(recoverAfter);
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 恢复序列协程
    /// </summary>
    private IEnumerator RecoverSequence()
    {
        isRecovering = true;
        
        // 重置位置和物理状态
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // 恢复到原始位置
        transform.position = originalPosition;
        transform.rotation = Quaternion.identity;
        
        // 淡入效果
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeIn());
        }
        
        // 触发恢复动画（如果有）
        if (animator != null)
        {
            animator.SetTrigger("Recover");
        }
        
        // 等待淡入完成
        yield return new WaitForSeconds(0.5f);
        
        // 恢复碰撞器
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }
        
        // 发布平台恢复事件
        EventManager.Instance.Publish(GameEventNames.PLATFORM_RECOVER, gameObject);
        
        // 重置状态
        isBreaking = false;
        isRecovering = false;
    }
    
    /// <summary>
    /// 淡入效果协程
    /// </summary>
    private IEnumerator FadeIn()
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsed = 0f;
        float fadeDuration = 0.5f;
        
        while (elapsed < fadeDuration)
        {
            Color newColor = spriteRenderer.color;
            newColor.a = Mathf.Lerp(startAlpha, 1, elapsed / fadeDuration);
            spriteRenderer.color = newColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
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
        
        while (elapsed < recoverAfter)
        {
            Color newColor = spriteRenderer.color;
            newColor.a = Mathf.Lerp(startAlpha, 0, elapsed / recoverAfter);
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
    
    /// <summary>
    /// 处理Boss阶段变化事件
    /// 在非Boss关中才响应此事件触发断裂
    /// </summary>
    private void OnBossPhaseChange(object data)
    {
        // 只有在非Boss关并且平台可以恢复时，才通过阶段变化触发断裂
        // Boss关的断裂由PLATFORM_BREAK事件（特效结束后）触发
        if (data is int phaseNumber && !isBossLevel && canRecover && !isBreaking && !isRecovering)
        {
            Debug.Log($"检测到Boss进入第{phaseNumber}阶段，触发平台断裂");
            StartCoroutine(BreakSequence());
        }
    }
    
    /// <summary>
    /// 处理地面断开事件
    /// 在Boss关通过此事件触发，非Boss关根据canRecover决定
    /// </summary>
    private void OnGroundBreak(object data)
    {
        // Boss关：直接触发断裂，无需考虑canRecover
        // 非Boss关：只有canRecover为true时才触发
        bool shouldBreak = isBossLevel || (canRecover && !isBreaking && !isRecovering);
        
        if (shouldBreak && !isBreaking && !isRecovering)
        {
            Debug.Log($"{(isBossLevel ? "Boss关" : "普通关")}检测到平台断裂事件，触发平台断裂");
            ForceBreak();
        }
    }
}