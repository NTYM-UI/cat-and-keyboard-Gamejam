using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonEscape : MonoBehaviour, IPointerMoveHandler,IPointerExitHandler
{
    private RectTransform btnRect;
    private RectTransform canvasRect;
    //public float moveCoolDown = 0.2f; // �ƶ���ȴ
    //private bool canMove = true; // ��ȴ���ƿ���
    public float escapeSpeed=2f;

    Vector2 mouseUIPos;

    void Start()
    {
        btnRect = GetComponent<RectTransform>();
        canvasRect = transform.parent.GetComponent<RectTransform>();
    }

    void Update()
    {
        //�洢ת��������UI�ֲ�����
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,                // UI���ڵ��RectTransform
            Input.mousePosition,       // ��Ļ����ϵ���λ��
            null,                      // Canvas��Overlayģʽ����null
            out mouseUIPos             // ���UI�ֲ�����
        );
    }

    private void MoveToRandomPosition()
    {
        Vector2 canvasMin = canvasRect.rect.min;
        Vector2 canvasMax = canvasRect.rect.max;

        //���㰴ť���ƶ��ı߽�
        float btnWidth = btnRect.rect.width;
        float btnHeight = btnRect.rect.height;
        float mouseUIPosX = mouseUIPos.x;
        float mouseUIPosY = mouseUIPos.y;
        float minX = canvasMin.x + btnWidth;
        float maxX = canvasMax.x - btnWidth;
        float minY = canvasMin.y + btnHeight;
        float maxY = canvasMax.y - btnHeight;

        //����������겢��ֵ����ť
        float randomX;
        float randomY;
        if (mouseUIPosX - btnRect.rect.min.x < btnRect.rect.max.x - mouseUIPosX)
            randomX= Random.Range(mouseUIPosX, maxX);
        else
            randomX = Random.Range(minX, mouseUIPosX);

        if (mouseUIPosY - btnRect.rect.min.y < btnRect.rect.max.y - mouseUIPosY)
            randomY = Random.Range(mouseUIPosY, maxY);
        else
            randomY = Random.Range(minY, mouseUIPosY);

        btnRect.anchoredPosition = new Vector2(Mathf.Lerp(btnRect.anchoredPosition.x, randomX, Time.deltaTime*escapeSpeed),
            Mathf.Lerp(btnRect.anchoredPosition.y, randomY, Time.deltaTime*escapeSpeed));
    }

    //��ȴЭ��
    //private IEnumerator SetMoveCoolDown()
    //{
    //    canMove = false;
    //    yield return new WaitForSeconds(moveCoolDown);
    //    canMove = true;
    //}

    public void OnPointerMove(PointerEventData eventData)
    {
        MoveToRandomPosition();
    }

    //�������Ƴ�����Ļ���������¼�����д������
    public void OnPointerExit(PointerEventData eventData)
    {

    }
}