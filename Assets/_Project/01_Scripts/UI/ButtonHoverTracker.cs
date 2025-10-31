using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 按钮悬停跟踪器
/// 当鼠标悬停在按钮上时，将指定图标移动到按钮左侧
/// </summary>
public class ButtonHoverTracker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("要跟随的选择图标")]
    public RectTransform selectionIcon;
    
    [Tooltip("图标与按钮左侧的水平间距")]
    public float horizontalSpacing = -20f;
    
    [Tooltip("图标与按钮的垂直偏移")]
    public float verticalOffset = 0f;
    
    [Tooltip("是否在游戏开始时隐藏图标")]
    public bool hideOnStart = false;
    
    private RectTransform buttonRect;
    private static ButtonHoverTracker currentHoveredButton;

    private void Start()
    {
        // 获取按钮的RectTransform
        buttonRect = GetComponent<RectTransform>();
        
        // 如果没有指定图标，尝试在场景中查找
        if (selectionIcon == null)
        {
            GameObject iconObj = GameObject.Find("SelectionIcon");
            if (iconObj != null)
            {
                selectionIcon = iconObj.GetComponent<RectTransform>();
            }
        }
        
        // 初始隐藏图标
        if (hideOnStart && selectionIcon != null)
        {
            selectionIcon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 当鼠标进入按钮区域时调用
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectionIcon != null && buttonRect != null)
        {
            // 更新当前悬停的按钮
            currentHoveredButton = this;
            
            // 移动图标到按钮左侧
            UpdateIconPosition();
            
            // 显示图标
            selectionIcon.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 当鼠标离开按钮区域时调用
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 检查是否还有其他按钮被悬停
        if (currentHoveredButton == this && selectionIcon != null)
        {
            // 隐藏图标
            // selectionIcon.gameObject.SetActive(false);
            currentHoveredButton = null;
        }
    }

    public void OnMouseDown()
    {
        selectionIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// 更新图标位置到按钮左侧
    /// </summary>
    private void UpdateIconPosition()
    {
        if (selectionIcon == null || buttonRect == null) return;
        
        // 计算图标位置：按钮左侧 + 水平间距
        Vector2 buttonPos = buttonRect.anchoredPosition;
        float iconX = buttonPos.x - buttonRect.rect.width / 2 + horizontalSpacing - selectionIcon.rect.width / 2;
        float iconY = buttonPos.y + verticalOffset;
        
        // 设置图标位置
        selectionIcon.anchoredPosition = new Vector2(iconX, iconY);
    }

    /// <summary>
    /// 强制更新图标位置（可从其他脚本调用）
    /// </summary>
    public void ForceUpdatePosition()
    {
        UpdateIconPosition();
    }
}