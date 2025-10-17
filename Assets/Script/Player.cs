using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBasic : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 6f;

    [Header("跳跃")]
    public float jumpHeightMul = 1f;
    public float rayLength = 0.55f;   // 射线长度（比角色半身略长即可）
    public LayerMask groundLayer;

    float jumpVel;
    float g => Physics2D.gravity.magnitude;

    Rigidbody2D rb;
    float horizontal;
    bool isGrounded;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            ComputeJumpVel();
            rb.velocity = new Vector2(rb.velocity.x, jumpVel);
        }
    }

    void FixedUpdate()
    {
        // 水平移动
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // 地面检测从脚底向下射线
        // 计算当前脚底位置（中心 - 半身高）
        float halfHeight = GetComponent<CapsuleCollider2D>().size.y * transform.lossyScale.y * 0.5f;
        Vector3 start = transform.position - Vector3.up * halfHeight;

        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, rayLength, groundLayer);
        isGrounded = hit.collider != null;
    }

    public void ComputeJumpVel()
    {
        jumpVel = Mathf.Sqrt(2f * g * transform.lossyScale.y * jumpHeightMul);
    }

    // 可视化射线
    void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.black : Color.white;

        float halfHeight = GetComponent<CapsuleCollider2D>().size.y * transform.lossyScale.y * 0.5f;
        Vector3 start = transform.position - Vector3.up * halfHeight;
        
        Gizmos.DrawLine(start, start + Vector3.down * rayLength);
    }
}