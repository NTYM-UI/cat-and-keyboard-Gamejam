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
        //����ť��������������ʹ��������ԣ���ֵ�ɵ�
        rb.gravityScale = 20;
        //����ť��һ�����µ����������������,��ֵ�ɵ���
        rb.AddForce(Vector2.down * 800f);
        //���ð�ť���,��ֹ�ظ����
        GetComponent<Button>().interactable = false;
    }
}