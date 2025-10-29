using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// 武器按钮脚本
/// 处理玩家与按钮的交互逻辑，使用BoxCollider和标签检测，并添加面积检测功能
/// </summary>
public class WeaponButton : MonoBehaviour
{
    [Header("视觉效果")]
    [SerializeField] private GameObject pressEffectPrefab; // 按下效果预制体
    [SerializeField] private SpriteRenderer spriteRenderer; // 按钮的精灵渲染器
    [SerializeField] private Color normalColor = Color.white; // 正常状态颜色
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f); // 按下状态颜色
    [SerializeField] private float pressAnimationDuration = 0.2f; // 按下动画持续时间
    [SerializeField] private float buttonPressDistance = 0.2f; // 按钮按下垂直位移
    [SerializeField] private float buttonReleaseDelay = 0.2f; // 按钮弹起时间延迟

    [Header("音效")]
    [SerializeField] private AudioClip pressSound; // 按下音效

    [Header("交互设置")]
    [SerializeField] private LayerMask playerLayer; // 玩家所在的层级
    [SerializeField] private bool useBoxCollider = true; // 是否使用BoxCollider进行检测
    [SerializeField] private Vector2 colliderSize = new Vector2(2f, 1f); // BoxCollider的大小

    [Header("面积检测设置")]
    [SerializeField] private float playerArea; // 玩家当前面积
    [SerializeField] private float buttonTargetArea = 1f; // 按钮目标面积（玩家面积需要大于此值才能触发）

    [Header("事件")]
    [SerializeField] private UnityEvent onButtonPressed; // 按钮按下时触发的事件

    private bool isPressed = false; // 按钮是否已被按下
    private bool isActivated = false; // 按钮是否已激活
    private float pressStartTime; // 按下开始时间
    private Vector3 originalScale; // 按钮原始缩放
    private Vector3 originalPosition; // 按钮原始位置
    private Vector3 pressedScale = new Vector3(0.9f, 0.9f, 0.9f); // 按下时的缩放
    private Coroutine releaseButtonCoroutine = null; // 释放按钮的协程引用

    private void Start()
    {
        // 记录原始缩放和位置
        originalScale = transform.localScale;
        originalPosition = transform.position;

        // 如果没有设置精灵渲染器，尝试获取组件
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // 设置初始颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        // 添加碰撞器（如果没有）
        Collider2D existingCollider = GetComponent<Collider2D>();
        if (existingCollider == null)
        {
            if (useBoxCollider)
            {
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = colliderSize;
                boxCollider.isTrigger = false; // 使用物理碰撞而非触发器
                
                // 添加刚体组件以支持物理碰撞
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Static;
                }
            }
            else
            {
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = colliderSize;
                boxCollider.isTrigger = true;
            }
        }
        else if (useBoxCollider && !(existingCollider is BoxCollider2D))
        {
            // 如果已有碰撞器但不是BoxCollider，替换它
            Destroy(existingCollider);
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = colliderSize;
            boxCollider.isTrigger = false;
            
            // 确保有刚体组件
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
        
        // 订阅窗口缩放变化事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe(GameEventNames.WINDOW_SCALE_CHANGED, OnWindowScaleChanged);
        }
    }
    
    private void OnEnable()
    {
        // 重新订阅事件
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

    private void Update()
    {
        // 如果按钮正在按下动画中，更新动画
        if (isPressed)
        {
            UpdatePressAnimation();
        }
        // 如果使用触发器模式，则继续使用Update进行检测
        else if (GetComponent<Collider2D>() != null && GetComponent<Collider2D>().isTrigger)
        {
            CheckPlayerTriggerInteraction();
        }
    }

    /// <summary>
    /// 检查玩家触发器交互（当使用isTrigger=true时）
    /// </summary>
    private void CheckPlayerTriggerInteraction()
    {
        // 使用矩形检测查找玩家
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Vector2 position = collider.bounds.center;
            Vector2 size = collider.bounds.size;
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(position, size, 0f, playerLayer);
            
            foreach (Collider2D hit in hits)
            {
                // 检查是否是玩家并满足面积条件
                if (hit.CompareTag("Player") && CheckAreaCondition())
                {
                    OnPlayerTrigger();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 玩家进入碰撞器时调用（使用物理碰撞）
    /// </summary>
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
                
                // 首次激活时触发功能
                if (!isActivated)
                {
                    OnPlayerTrigger();
                }
            }
        }
    }
    
    /// <summary>
    /// 玩家持续踩踏时调用（使用物理碰撞）
    /// </summary>
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
                
                // 首次激活时触发功能
                if (!isActivated)
                {
                    OnPlayerTrigger();
                }
            }
        }
    }
    
    /// <summary>
    /// 玩家离开按钮时调用（使用物理碰撞）
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
    /// 按钮按下效果
    /// </summary>
    private void PressButton()
    {
        if (isPressed) return;
        
        isPressed = true;
        transform.position = originalPosition - new Vector3(0, buttonPressDistance, 0);
    }
    
    /// <summary>
    /// 延迟释放按钮的协程
    /// </summary>
    private IEnumerator ReleaseButtonAfterDelay()
    {
        // 使用配置的延迟时间
        yield return new WaitForSeconds(buttonReleaseDelay);
        
        // 检测玩家是否真的离开了
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Vector2 position = collider.bounds.center;
            Vector2 size = collider.bounds.size;
            Collider2D[] colliders = Physics2D.OverlapBoxAll(position, size, 0f, playerLayer);
            bool hasPlayerNearby = false;
            
            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    hasPlayerNearby = true;
                    break;
                }
            }
            
            // 确认无玩家时恢复按钮
            if (!hasPlayerNearby && isPressed)
            {
                transform.position = originalPosition;
                isPressed = false;
                isActivated = false;
            }
        }
        
        releaseButtonCoroutine = null;
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
    private void OnWindowScaleChanged(object data)
    {
        // 将object类型参数转换为Vector3
        if (data is Vector3 newScale)
        {
            playerArea = newScale.x * newScale.y;
        }
    }

    /// <summary>
    /// 玩家触发按钮
    /// </summary>
    private void OnPlayerTrigger()
    {
        if (isActivated) return;

        isActivated = true;
        pressStartTime = Time.time;

        // 播放按下动画
        StartPressAnimation();

        // 播放按下音效
        if (pressSound != null)
        {
            AudioSource.PlayClipAtPoint(pressSound, transform.position);
        }

        // 发布按钮按下事件
        EventManager.Instance.Publish(GameEventNames.BOSS_WEAPON_BUTTON_PRESSED, gameObject);

        // 触发Unity事件
        onButtonPressed?.Invoke();

        Debug.Log("武器按钮被玩家触发 (满足面积条件: " + playerArea + " > " + buttonTargetArea + ")");
    }

    /// <summary>
    /// 开始按下动画
    /// </summary>
    private void StartPressAnimation()
    {
        // 立即设置颜色为按下颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = pressedColor;
        }
    }

    /// <summary>
    /// 更新按下动画
    /// </summary>
    private void UpdatePressAnimation()
    {
        float elapsedTime = Time.time - pressStartTime;
        float progress = Mathf.Clamp01(elapsedTime / pressAnimationDuration);

        // 实现按钮按下的缩放动画
        transform.localScale = Vector3.Lerp(originalScale, pressedScale, progress);

        // 如果动画完成，可以选择禁用按钮
        if (progress >= 1f)
        {
            // 可以在这里添加额外的逻辑，比如禁用按钮的交互
            enabled = false;
        }
    }

    /// <summary>
    /// 在编辑器中显示按钮的交互范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        
        // 显示BoxCollider范围
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
        }
        else
        {
            // 如果没有collider组件，使用配置的大小
            Gizmos.DrawCube(transform.position, colliderSize);
        }
    }

    /// <summary>
    /// 重置按钮状态（如果需要复用）
    /// </summary>
    public void ResetButton()
    {
        isPressed = false;
        isActivated = false;
        transform.localScale = originalScale;
        transform.position = originalPosition;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        
        enabled = true;
    }
}