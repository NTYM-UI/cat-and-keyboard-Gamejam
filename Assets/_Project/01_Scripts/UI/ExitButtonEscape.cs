using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonEscape : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler, IPointerDownHandler
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
        float btnWidth = btnRect.rect.width / 2; // 使用一半宽度作为边界，确保按钮完全在屏幕内
        float btnHeight = btnRect.rect.height / 2; // 使用一半高度作为边界，确保按钮完全在屏幕内
        float mouseUIPosX = mouseUIPos.x;
        float mouseUIPosY = mouseUIPos.y;
        float minX = canvasMin.x + btnWidth;
        float maxX = canvasMax.x - btnWidth;
        float minY = canvasMin.y + btnHeight;
        float maxY = canvasMax.y - btnHeight;

        // 计算当前按钮位置
        float currentX = btnRect.anchoredPosition.x;
        float currentY = btnRect.anchoredPosition.y;

        // 生成随机方向偏移
        // 基于鼠标位置计算反方向，但增加随机因素确保不会总是同一方向
        float directionX = (currentX > mouseUIPosX) ? 1 : -1; // 如果按钮在鼠标右侧，则倾向于向右移动
        float directionY = (currentY > mouseUIPosY) ? 1 : -1; // 如果按钮在鼠标上方，则倾向于向上移动

        // 随机决定是否反转方向，增加不可预测性
        if (Random.value > 0.7f) directionX *= -1;
        if (Random.value > 0.7f) directionY *= -1;

        // 计算移动距离，确保有足够的移动空间
        float moveDistanceX = Random.Range(50f, 200f) * directionX;
        float moveDistanceY = Random.Range(50f, 200f) * directionY;

        // 计算目标位置
        float targetX = currentX + moveDistanceX;
        float targetY = currentY + moveDistanceY;

        // 确保最终位置不会超出边界
        float randomX = Mathf.Clamp(targetX, minX, maxX);
        float randomY = Mathf.Clamp(targetY, minY, maxY);
        
        // 如果因为边界限制导致方向被改变，再次随机化位置
        if (Mathf.Abs(randomX - targetX) > 10f) {
            randomX = Random.Range(minX, maxX);
        }
        if (Mathf.Abs(randomY - targetY) > 10f) {
            randomY = Random.Range(minY, maxY);
        }

        btnRect.anchoredPosition = new Vector2(Mathf.Lerp(btnRect.anchoredPosition.x, randomX, Time.deltaTime * escapeSpeed),
            Mathf.Lerp(btnRect.anchoredPosition.y, randomY, Time.deltaTime * escapeSpeed));
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
        // 鼠标悬停时不自动移动，只有按下时才移动
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // 当鼠标按下时，按钮开始逃离
        MoveToRandomPosition();
    }

    //如果鼠标移出了屏幕，触发的事件可以写在这里
    public void OnPointerExit(PointerEventData eventData)
    {

    }
}