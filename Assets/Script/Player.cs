using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private WindowScale windowScale;
    public static Player instance;

    [Header("移动")]
    public float moveSpeed = 6f;

    [Header("跳跃")]
    public float jumpHeightMul = 1f;
    public float rayLength = 0.55f; // 射线长度（用于检测地面）
    public LayerMask groundLayer;

    [Header("面积")]
    public float area;
    public float lerpSpeed; 

    private float jumpVel;
    private float g => Physics2D.gravity.magnitude;

    private float horizontal;
    private bool isGrounded;


    void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;

        rb = GetComponent<Rigidbody2D>();
        windowScale = FindObjectOfType<WindowScale>();

        if (windowScale != null)
        {
            windowScale.OnScaleChanged += OnWindowScaleChanged;
        }
    }

    void OnDestroy()
    {
        if (windowScale != null)
        {
            windowScale.OnScaleChanged -= OnWindowScaleChanged;
        }
    }

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

        // 地面检测（脚底射线）
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

    #region event

    private void OnWindowScaleChanged(Vector3 newScale)
    {
        transform.localScale = Vector3.Lerp(transform.localScale, newScale, lerpSpeed * Time.deltaTime);
        area = newScale.x * newScale.y;
    }

    #endregion
}