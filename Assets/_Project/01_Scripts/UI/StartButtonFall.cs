using UnityEngine;
using UnityEngine.UI;

public class StartButtonFall : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnStartButtonClick()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        //给按钮设置重力参数，使下落更明显，数值可调
        rb.gravityScale = 20;
        //给按钮加一点向下的力，让下落更明显,数值可调整
        rb.AddForce(Vector2.down * 800f);
        //禁用按钮点击,防止重复点击
        GetComponent<Button>().interactable = false;
    }
}