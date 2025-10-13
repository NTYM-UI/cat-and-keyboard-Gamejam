using UnityEngine;

/// <summary>
/// 玩家控制器 - 负责处理玩家的移动和跳跃逻辑
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private bool isReverseControl = false; // 反向控制状态
    
    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 7f; // 跳跃力
    [SerializeField] private int maxJumps = 1; // 最大跳跃次数
    [SerializeField] private float jumpCooldown = 0.2f; // 跳跃冷却时间
    [Header("物理设置")]
    [SerializeField] private float gravityScale = 2f; // 重力比例，初始值设为2
    
    [Header("地面检测")]
    [SerializeField] private Transform groundCheck; // 地面检测点
    [SerializeField] private float groundCheckRadius = 0.02f; // 检测半径
    [SerializeField] private LayerMask groundLayer; // 地面图层
    [SerializeField] private string groundLayerName = "Ground"; // 地面图层名称
    
    // 组件引用
    private Rigidbody2D rb; 
    private SpriteRenderer spriteRenderer; 
    
    // 状态变量
    private bool isGrounded = false; 
    private bool wasGrounded = false; 
    private bool isJumping = false; 
    private int currentJumps = 0; 
    private float lastJumpTime = -Mathf.Infinity; 
    private float moveDirection = 0f; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 设置重力比例和冻结Z轴旋转
        if (rb != null)
        {
            rb.gravityScale = gravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        if (!groundCheck)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.parent = transform;
            groundCheck.localPosition = new Vector3(0f, -0.5f, 0f);
        }
        
        // 在Awake中初始化LayerMask，避免序列化问题
        groundLayer = LayerMask.GetMask(groundLayerName);
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_MOVE, OnPlayerMove);
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_JUMP, OnPlayerJump);
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, OnToggleReverseControl);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_MOVE, OnPlayerMove);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_JUMP, OnPlayerJump);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, OnToggleReverseControl);
    }

    private void Update()
    {
        CheckGrounded();
        UpdateJumpState();
    }

    private void FixedUpdate() => ApplyMovement();

    /// <summary>处理玩家移动输入事件</summary>
    private void OnPlayerMove(object data)
    {
        if (data is float horizontalInput)
        {
            moveDirection = isReverseControl ? -horizontalInput : horizontalInput;
            if (moveDirection != 0) spriteRenderer.flipX = moveDirection < 0;
        }
    }

    /// <summary>处理跳跃事件</summary>
    private void OnPlayerJump(object data)
    {
        // 在反向跳跃模式下，只有按S键（Vertical轴负方向）才能跳跃
        if (isReverseControl)
        {
            if (Input.GetKeyDown(KeyCode.S)) Jump();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W)) Jump();
        }
    }

    /// <summary>处理反转控制切换事件</summary>
    private void OnToggleReverseControl(object data) => ToggleReverseControl();

    /// <summary>检查玩家是否在地面上并处理落地状态</summary>
    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (!wasGrounded && isGrounded) isJumping = false;
    }

    /// <summary>更新跳跃状态 - 落地时重置跳跃次数</summary>
    private void UpdateJumpState()
    {
        if (isGrounded && !isJumping) currentJumps = 0;
    }

    /// <summary>检查是否可以跳跃</summary>
    private bool CanJump()
    {
        if (currentJumps < 0) currentJumps = 0;
        return Time.time >= lastJumpTime + jumpCooldown && currentJumps < maxJumps;
    }

    /// <summary>执行跳跃动作</summary>
    private void Jump()
    {
        if (!CanJump()) return;
        
        isJumping = true;
        lastJumpTime = Time.time;
        currentJumps = Mathf.Min(currentJumps + 1, maxJumps);
        
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    /// <summary>应用移动力</summary>
    private void ApplyMovement()
    {
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    /// <summary>切换反向控制状态</summary>
    private void ToggleReverseControl()
    {
        isReverseControl = !isReverseControl;
        Debug.Log("玩家控制模式切换为：" + (isReverseControl ? "反向" : "正常"));
    }

    // 公共API方法
    public void SetMoveSpeed(float newSpeed) => moveSpeed = newSpeed;
    public void SetJumpForce(float newJumpForce) => jumpForce = newJumpForce;
    public bool IsReverseControlEnabled() => isReverseControl;
    public void SetReverseControl(bool isReverse) => isReverseControl = isReverse;

    /// <summary>在编辑器中绘制地面检测范围的gizmo</summary>
    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}