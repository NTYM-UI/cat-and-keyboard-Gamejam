using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 退出按钮逃离
/// </summary>
public class InteractiveButton : MonoBehaviour, IPointerDownHandler
{
    private RectTransform btnRect;
    private RectTransform canvasRect;
    public float escapeSpeed = 500f;
    private Vector2 mousePos;
    private int clickCount = 0;
    private bool isInDialog = false; // 标记是否正在显示对话
    private bool isButtonHidden = false; // 标记按钮是否已隐藏
    private const int REQUIRED_CLICKS_FOR_HIDDEN_DIALOG = 3; // 触发隐藏对话的点击次数

    private void OnEnable()
    {
        // 订阅对话开始和结束事件
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_START, OnDialogStart);
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }

    private void OnDisable()
    {
        // 取消订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_START, OnDialogStart);
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
        }
    }

    void Start()
    {
        btnRect = GetComponent<RectTransform>();
        canvasRect = transform.parent.GetComponent<RectTransform>();
    }

    void Update()
    {
        // 获取鼠标在UI局部坐标系中的位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, Input.mousePosition, null, out mousePos);
    }

    private void MoveToRandomPosition()
    {
        // 计算边界和当前位置
        float btnWidth = btnRect.rect.width / 2;
        float btnHeight = btnRect.rect.height / 2;
        float minX = canvasRect.rect.min.x + btnWidth;
        float maxX = canvasRect.rect.max.x - btnWidth;
        float minY = canvasRect.rect.min.y + btnHeight;
        float maxY = canvasRect.rect.max.y - btnHeight;

        Vector2 currentPos = btnRect.anchoredPosition;

        // 计算逃离方向并添加随机性
        float dirX = (currentPos.x > mousePos.x) ? 1 : -1;
        float dirY = (currentPos.y > mousePos.y) ? 1 : -1;
        if (Random.value > 0.7f) dirX *= -1;
        if (Random.value > 0.7f) dirY *= -1;

        // 计算目标位置
        float targetX = Mathf.Clamp(currentPos.x + Random.Range(50f, 200f) * dirX, minX, maxX);
        float targetY = Mathf.Clamp(currentPos.y + Random.Range(50f, 200f) * dirY, minY, maxY);

        // 如果边界限制太大，则完全随机位置
        if (Mathf.Abs(targetX - (currentPos.x + dirX * 100f)) > 10f)
            targetX = Random.Range(minX, maxX);
        if (Mathf.Abs(targetY - (currentPos.y + dirY * 100f)) > 10f)
            targetY = Random.Range(minY, maxY);

        // 平滑移动到目标位置
        btnRect.anchoredPosition = Vector2.Lerp(currentPos, new Vector2(targetX, targetY), Time.deltaTime * escapeSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 如果按钮已隐藏或正在对话中，则不响应点击
        if (isButtonHidden || isInDialog)
            return;



        // 处理点击计数和交互逻辑
        clickCount++;
        Debug.Log("退出按钮点击次数: " + clickCount);

        if (clickCount <= REQUIRED_CLICKS_FOR_HIDDEN_DIALOG)
        {
            // 前三次点击触发隐藏对话
            TriggerHiddenDialog();
        }
        else if (clickCount == REQUIRED_CLICKS_FOR_HIDDEN_DIALOG + 1)
        {
            // 第四次点击 - 隐藏按钮并显示退出确认对话
            HideButtonAndShowExitConfirmation();
        }
    }

    /// <summary>
    /// 触发对话
    /// </summary>
    private void TriggerHiddenDialog()
    {
        if (EventManager.Instance != null)
        {
            // 根据点击次数使用不同的对话ID
            int dialogId = GetDialogIdByClickCount(clickCount);
            EventManager.Instance.Publish(GameEventNames.DIALOG_START, dialogId);
        }
    }

    /// <summary>
    /// 根据点击次数获取对应的对话ID
    /// </summary>
    /// <param name="count">点击次数</param>
    /// <returns>对话ID字符串</returns>
    private int GetDialogIdByClickCount(int count)
    {
        switch (count)
        {
            case 1:
                return 4;
            case 2:
                return 6;
            case 3:
                return 8;
            default:
                return 10;
        }
    }

    /// <summary>
    /// 隐藏按钮并显示退出确认对话
    /// </summary>
    private void HideButtonAndShowExitConfirmation()
    {
        // 隐藏按钮
        gameObject.SetActive(false);
        isButtonHidden = true;
        Debug.Log("按钮已隐藏，显示退出确认对话");

        // 触发退出确认对话
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Publish(GameEventNames.DIALOG_START, 10);
        }
    }

    /// <summary>
    /// 处理对话开始事件
    /// </summary>
    private void OnDialogStart(object data)
    {
        isInDialog = true;
        if (data is 6 || data is 8)
        {
            // 按钮逃离
            MoveToRandomPosition();
        }
    }

    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    private void OnDialogEnd(object data)
    {
        isInDialog = false;
        if (data is 5)
        {
            // 按钮逃离
            MoveToRandomPosition();
        }
    }
}