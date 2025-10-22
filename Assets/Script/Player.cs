using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBasic : MonoBehaviour
{
    [Header("�ƶ�")]
    public float moveSpeed = 6f;

    [Header("��Ծ")]
    public float jumpHeightMul = 1f;
    public float rayLength = 0.55f;   // ���߳��ȣ��Ƚ�ɫ�����Գ����ɣ�
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

        // ��Ծ
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            ComputeJumpVel();
            rb.velocity = new Vector2(rb.velocity.x, jumpVel);
        }
    }

    void FixedUpdate()
    {
        // ˮƽ�ƶ�
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // ������ӽŵ���������
        // ���㵱ǰ�ŵ�λ�ã����� - ����ߣ�
        float halfHeight = GetComponent<CapsuleCollider2D>().size.y * transform.lossyScale.y * 0.5f;
        Vector3 start = transform.position - Vector3.up * halfHeight;

        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, rayLength, groundLayer);
        isGrounded = hit.collider != null;
    }

    public void ComputeJumpVel()
    {
        jumpVel = Mathf.Sqrt(2f * g * transform.lossyScale.y * jumpHeightMul);
    }

    // ���ӻ�����
    void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.black : Color.white;

        float halfHeight = GetComponent<CapsuleCollider2D>().size.y * transform.lossyScale.y * 0.5f;
        Vector3 start = transform.position - Vector3.up * halfHeight;
        
        Gizmos.DrawLine(start, start + Vector3.down * rayLength);
    }
}