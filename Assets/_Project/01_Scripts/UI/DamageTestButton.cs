using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 伤害测试按钮
/// 用于在Unity编辑器中测试玩家生命值系统
/// </summary>
public class DamageTestButton : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button damageButton; // 伤害按钮
    [SerializeField] private TextMeshProUGUI buttonText; // 按钮文本
    [SerializeField] private string buttonLabel = "受到伤害"; // 按钮标签
    
    [Header("伤害设置")]
    [SerializeField] private int damageAmount = 1; // 每次点击造成的伤害
    
    [Header("恢复设置")]
    [SerializeField] private bool showHealButton = true; // 是否显示恢复按钮
    [SerializeField] private Button healButton; // 恢复按钮
    [SerializeField] private TextMeshProUGUI healButtonText; // 恢复按钮文本
    [SerializeField] private string healButtonLabel = "恢复生命"; // 恢复按钮标签
    [SerializeField] private int healAmount = 1; // 每次点击恢复的生命值
    
    [Header("调试信息")]
    [SerializeField] private TextMeshProUGUI debugText; // 调试信息文本
    [SerializeField] private bool showDebugInfo = true; // 是否显示调试信息
    
    private PlayerHealth playerHealth; // 玩家生命值组件

    private void Awake()
    {
        // 尝试查找玩家生命值组件
        FindPlayerHealth();
        
        // 初始化UI元素
        InitializeUI();
    }

    private void OnEnable()
    {
        // 如果找不到玩家生命值组件，尝试再次查找
        if (playerHealth == null)
        {
            FindPlayerHealth();
        }
        
        // 更新调试信息
        UpdateDebugInfo();
    }

    /// <summary>
    /// 查找玩家生命值组件
    /// </summary>
    private void FindPlayerHealth()
    {
        // 尝试查找场景中的PlayerHealth组件
        playerHealth = FindObjectOfType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogWarning("没有找到PlayerHealth组件，请确保玩家对象上已添加该组件");
        }
    }

    /// <summary>
    /// 初始化UI元素
    /// </summary>
    private void InitializeUI()
    {
        // 设置伤害按钮
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(OnDamageButtonClicked);
            if (buttonText != null)
            {
                buttonText.text = $"{buttonLabel} (-{damageAmount})".Trim();
            }
        }
        
        // 设置恢复按钮
        if (healButton != null)
        {
            healButton.onClick.AddListener(OnHealButtonClicked);
            if (healButtonText != null)
            {
                healButtonText.text = $"{healButtonLabel} (+{healAmount})".Trim();
            }
            // 根据设置显示或隐藏恢复按钮
            healButton.gameObject.SetActive(showHealButton);
        }
    }

    /// <summary>
    /// 伤害按钮点击事件
    /// </summary>
    public void OnDamageButtonClicked()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log($"模拟受到{damageAmount}点伤害");
        }
        else
        {
            Debug.LogWarning("无法模拟伤害：未找到PlayerHealth组件");
        }
        
        // 更新调试信息
        UpdateDebugInfo();
    }

    /// <summary>
    /// 恢复按钮点击事件
    /// </summary>
    public void OnHealButtonClicked()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            Debug.Log($"模拟恢复{healAmount}点生命值");
        }
        else
        {
            Debug.LogWarning("无法模拟恢复：未找到PlayerHealth组件");
        }
        
        // 更新调试信息
        UpdateDebugInfo();
    }

    /// <summary>
    /// 更新调试信息
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (debugText != null && showDebugInfo)
        {
            if (playerHealth != null)
            {
                int currentHealth = playerHealth.GetCurrentHealth();
                int maxHealth = playerHealth.GetMaxHealth();
                debugText.text = $"当前生命值: {currentHealth}/{maxHealth}";
            }
            else
            {
                debugText.text = "未找到PlayerHealth组件";
            }
        }
    }

    /// <summary>
    /// 手动刷新玩家生命值引用
    /// </summary>
    [ContextMenu("刷新玩家生命值引用")]
    public void RefreshPlayerHealth()
    {
        FindPlayerHealth();
        UpdateDebugInfo();
    }

    /// <summary>
    /// 自动创建测试按钮UI
    /// 在编辑器中通过右键菜单调用
    /// </summary>
    [ContextMenu("创建测试按钮UI")]
    public void CreateTestUI()
    {
        // 查找或创建画布
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("TestCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 创建按钮容器
        GameObject containerObj = new GameObject("DamageTestContainer");
        containerObj.transform.SetParent(canvas.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 20);
        containerRect.sizeDelta = new Vector2(200, 100);

        // 创建垂直布局组
        VerticalLayoutGroup layoutGroup = containerObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // 创建伤害按钮
        GameObject damageButtonObj = new GameObject("DamageButton");
        damageButtonObj.transform.SetParent(containerRect, false);
        damageButton = damageButtonObj.AddComponent<Button>();
        damageButton.targetGraphic = damageButtonObj.AddComponent<Image>();
        damageButton.targetGraphic.color = new Color(0.8f, 0.2f, 0.2f, 1f); // 红色
        
        // 创建伤害按钮文本
        GameObject damageTextObj = new GameObject("DamageButtonText");
        damageTextObj.transform.SetParent(damageButtonObj.transform, false);
        buttonText = damageTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = $"{buttonLabel} (-{damageAmount})".Trim();
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        
        // 创建恢复按钮（如果启用）
        if (showHealButton)
        {
            GameObject healButtonObj = new GameObject("HealButton");
            healButtonObj.transform.SetParent(containerRect, false);
            healButton = healButtonObj.AddComponent<Button>();
            healButton.targetGraphic = healButtonObj.AddComponent<Image>();
            healButton.targetGraphic.color = new Color(0.2f, 0.8f, 0.2f, 1f); // 绿色
            
            // 创建恢复按钮文本
            GameObject healTextObj = new GameObject("HealButtonText");
            healTextObj.transform.SetParent(healButtonObj.transform, false);
            healButtonText = healTextObj.AddComponent<TextMeshProUGUI>();
            healButtonText.text = $"{healButtonLabel} (+{healAmount})".Trim();
            healButtonText.alignment = TextAlignmentOptions.Center;
            healButtonText.fontSize = 16;
            healButtonText.color = Color.white;
        }

        // 创建调试信息文本
        if (showDebugInfo)
        {
            GameObject debugTextObj = new GameObject("DebugText");
            debugTextObj.transform.SetParent(containerRect, false);
            debugText = debugTextObj.AddComponent<TextMeshProUGUI>();
            debugText.alignment = TextAlignmentOptions.Center;
            debugText.fontSize = 14;
            debugText.color = Color.white;
        }

        // 重新初始化UI
        InitializeUI();
        UpdateDebugInfo();
        
        Debug.Log("伤害测试按钮UI创建完成");
    }
}