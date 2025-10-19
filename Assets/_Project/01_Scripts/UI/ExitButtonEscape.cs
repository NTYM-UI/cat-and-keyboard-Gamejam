using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonEscape : MonoBehaviour, IPointerMoveHandler,IPointerExitHandler
{
    private RectTransform btnRect;
    private RectTransform canvasRect;
    //public float moveCoolDown = 0.2f; // 移动冷却
    //private bool canMove = true; // 冷却控制开关
    public float escapeSpeed=2f;

    Vector2 mouseUIPos;

    void Start()
    {
        btnRect = GetComponent<RectTransform>();
        canvasRect = transform.parent.GetComponent<RectTransform>();
    }

    void Update()
    {
        //存储转换后的鼠标UI局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,                // UI根节点的RectTransform
            Input.mousePosition,       // 屏幕坐标系鼠标位置
            null,                      // Canvas是Overlay模式，传null
            out mouseUIPos             // 输出UI局部坐标
        );
    }

    private void MoveToRandomPosition()
    {
        Vector2 canvasMin = canvasRect.rect.min;
        Vector2 canvasMax = canvasRect.rect.max;

        //计算按钮可移动的边界
        float btnWidth = btnRect.rect.width;
        float btnHeight = btnRect.rect.height;
        float mouseUIPosX = mouseUIPos.x;
        float mouseUIPosY = mouseUIPos.y;
        float minX = canvasMin.x + btnWidth;
        float maxX = canvasMax.x - btnWidth;
        float minY = canvasMin.y + btnHeight;
        float maxY = canvasMax.y - btnHeight;

        //生成随机坐标并赋值给按钮
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

    //冷却协程
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

    //如果鼠标移出了屏幕，触发的事件可以写在这里
    public void OnPointerExit(PointerEventData eventData)
    {

    }
}