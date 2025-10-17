using UnityEngine;

/// <summary>
/// 玩家控制器 - 负责处理玩家的移动和跳跃逻辑
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;         // 移动速度
    [SerializeField] private bool isReverseControl = false; // 反向控制状态
    
    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 7f;          // 跳跃力
    [SerializeField] private int maxJumps = 1;              // 最大跳跃次数
    [SerializeField] private float jumpCooldown = 0.2f;     // 跳跃冷却时间
    
    [Header("物理设置")]
    [SerializeField] private float gravityScale = 2f;       // 重力比例
    
    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;         // 地面检测点
    [SerializeField] private float groundCheckRadius = 0.02f; // 检测半径
    [SerializeField] private LayerMask groundLayer;         // 地面图层
    [SerializeField] private string groundLayerName = "Ground"; // 地面图层名称
    
    // 墙体检测相关变量
    [Header("墙体检测设置")]
    [SerializeField] private float wallCheckDistance = 0.26f; // 墙体检测距离
    [SerializeField] private LayerMask wallLayer;           // 墙体图层
    [SerializeField] private string wallLayerName = "Ground"; // 墙体图层名称
    [SerializeField] private float wallCheckHeightOffset = 0.3f; // 墙体检测线高度偏移
    
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
    private bool isTouchingWall = false; // 是否接触墙体
    private float wallDirection = 0f;    // 墙体方向（-1:左, 1:右） 

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
        
        // 创建默认地面检测点
        if (!groundCheck)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.parent = transform;
            groundCheck.localPosition = new Vector3(0f, -0.5f, 0f);
        }
        
        // 初始化LayerMask
        groundLayer = LayerMask.GetMask(groundLayerName);
        wallLayer = LayerMask.GetMask(wallLayerName);
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
        if (isReverseControl)
            { if (Input.GetKeyDown(KeyCode.S)) Jump(); }
        else
            { if (Input.GetKeyDown(KeyCode.W)) Jump(); }
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
        return isGrounded && Time.time >= lastJumpTime + jumpCooldown && currentJumps < maxJumps;
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
        // 检测是否接触墙体
        CheckWall();
        
        // 处理墙体碰撞逻辑
        float targetHorizontalSpeed = moveDirection * moveSpeed;
        
        // 当接触墙体时，防止向墙体方向移动（移除!isGrounded限制）
        if (isTouchingWall && Mathf.Sign(moveDirection) == wallDirection)
            targetHorizontalSpeed = 0f;
        
        rb.velocity = new Vector2(targetHorizontalSpeed, rb.velocity.y);
    }
    
    /// <summary>检测是否接触墙体</summary>
    private void CheckWall()
    {
        // 定义多个检测点，覆盖角色不同高度位置
        Vector2 leftCheckPosCenter = transform.position;
        Vector2 rightCheckPosCenter = transform.position;
        Vector2 leftCheckPosTop = new Vector2(transform.position.x, transform.position.y + wallCheckHeightOffset);
        Vector2 rightCheckPosTop = new Vector2(transform.position.x, transform.position.y + wallCheckHeightOffset);
        Vector2 leftCheckPosBottom = new Vector2(transform.position.x, transform.position.y - wallCheckHeightOffset);
        Vector2 rightCheckPosBottom = new Vector2(transform.position.x, transform.position.y - wallCheckHeightOffset);
        
        // 发射多条射线检测墙体
        RaycastHit2D leftHitCenter = Physics2D.Raycast(leftCheckPosCenter, Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D rightHitCenter = Physics2D.Raycast(rightCheckPosCenter, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D leftHitTop = Physics2D.Raycast(leftCheckPosTop, Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D rightHitTop = Physics2D.Raycast(rightCheckPosTop, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D leftHitBottom = Physics2D.Raycast(leftCheckPosBottom, Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D rightHitBottom = Physics2D.Raycast(rightCheckPosBottom, Vector2.right, wallCheckDistance, wallLayer);
        
        // 检测是否接触墙体
        isTouchingWall = leftHitCenter || rightHitCenter || leftHitTop || rightHitTop || leftHitBottom || rightHitBottom;
        
        // 更新墙体方向
        if (leftHitCenter || leftHitTop || leftHitBottom)
            wallDirection = -1f; // 左侧墙体
        else if (rightHitCenter || rightHitTop || rightHitBottom)
            wallDirection = 1f; // 右侧墙体
        else
            wallDirection = 0f; // 无墙体
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

    /// <summary>在编辑器中绘制调试gizmo</summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制地面检测范围
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // 绘制墙体检测射线
        Vector2 leftCheckPosCenter = transform.position;
        Vector2 rightCheckPosCenter = transform.position;
        Vector2 leftCheckPosTop = new Vector2(transform.position.x, transform.position.y + wallCheckHeightOffset);
        Vector2 rightCheckPosTop = new Vector2(transform.position.x, transform.position.y + wallCheckHeightOffset);
        Vector2 leftCheckPosBottom = new Vector2(transform.position.x, transform.position.y - wallCheckHeightOffset);
        Vector2 rightCheckPosBottom = new Vector2(transform.position.x, transform.position.y - wallCheckHeightOffset);
        
        Gizmos.color = isTouchingWall ? Color.red : Color.blue;
        // 绘制中心检测线
        Gizmos.DrawLine(leftCheckPosCenter, leftCheckPosCenter + Vector2.left * wallCheckDistance);
        Gizmos.DrawLine(rightCheckPosCenter, rightCheckPosCenter + Vector2.right * wallCheckDistance);
        // 绘制顶部检测线
        Gizmos.DrawLine(leftCheckPosTop, leftCheckPosTop + Vector2.left * wallCheckDistance);
        Gizmos.DrawLine(rightCheckPosTop, rightCheckPosTop + Vector2.right * wallCheckDistance);
        // 绘制底部检测线
        Gizmos.DrawLine(leftCheckPosBottom, leftCheckPosBottom + Vector2.left * wallCheckDistance);
        Gizmos.DrawLine(rightCheckPosBottom, rightCheckPosBottom + Vector2.right * wallCheckDistance);
    }
}