using UnityEngine;
using UnityEngine.UI;

public enum ButtonType 
{
    bigButton,
    smallButton
}

public class ButtonInteraction : MonoBehaviour
{
    private WindowScale windowScale;

    [Header("玩家相关")]
    [SerializeField] private float playerArea;

    [Header("按钮")]
    public ButtonType buttonType; // 按钮类型
    public float buttonTargetArea = 1; // 目标面积
    public bool buttonEnabled = false;
    private Color defaultColor;

    void Start()
    {
        defaultColor = GetComponent<SpriteRenderer>().color;
        windowScale = FindObjectOfType<WindowScale>();

        if (windowScale != null)
        {
            windowScale.OnScaleChanged += OnWindowScaleChanged;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            Player player = other.GetComponent<Player>();

            // 检查玩家面积条件
            if (CheckAreaCondition())
            {
                GetComponent<SpriteRenderer>().color = Color.green; // 按钮变绿表示可用
                buttonEnabled = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            GetComponent<SpriteRenderer>().color = defaultColor; // 按钮变红表示不可用
            buttonEnabled = false;
        }
    }

    private bool CheckAreaCondition()
    {
        if (buttonType == ButtonType.bigButton)
        {
            return playerArea > buttonTargetArea ? true : false;
        }
        else  
        {
            return playerArea < buttonTargetArea ? true : false;
        }
    }

    #region event

    private void OnWindowScaleChanged(Vector3 newScale)
    {
        playerArea = newScale.x * newScale.y;
    }

    #endregion
}