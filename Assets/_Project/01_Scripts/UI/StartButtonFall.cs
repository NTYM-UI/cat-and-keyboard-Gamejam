using UnityEngine;
using UnityEngine.UI;

public class StartButtonFall : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        // 获取自身或父物体上的Rigidbody2D
        rb = GetComponentInParent<Rigidbody2D>();
        // 初始时冻结Y轴
        rb.constraints = RigidbodyConstraints2D.FreezePositionY;
    }

    // 当鼠标点击时调用
    private void OnMouseDown()
    {
        if (rb != null)
        {
            // 解除Y轴冻结
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        }
    }
}