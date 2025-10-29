using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("生命值UI设置")]
    [SerializeField] private GameObject heartPrefab; // 爱心预制体
    [SerializeField] private Transform heartsContainer; // 爱心容器
    [SerializeField] private Sprite fullHeartSprite; // 完整爱心精灵
    [SerializeField] private Sprite emptyHeartSprite; // 空爱心精灵
    [SerializeField] private float heartSpacing = 20f; // 爱心间距
    
    [Header("Boss相关设置")]
    [SerializeField] private int bossDialogId = 110; // Boss出现的对话ID
    [SerializeField] private float fadeInDuration = 0.5f; // 爱心出现的淡入时间

    private List<Image> heartImages = new List<Image>(); // 存储所有爱心Image组件
    private int maxHealth; // 最大生命值
    private bool isBossAppeared = false; // Boss是否已经出现
    private CanvasGroup canvasGroup; // 用于控制淡入效果

    private void Awake()
    {
        // 获取CanvasGroup组件用于控制透明度
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 初始时设置透明度为0，隐藏UI
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        // 订阅生命值相关事件
        EventManager.Instance.Subscribe(GameEventNames.HEALTH_UI_INIT, OnHealthUIInit);
        EventManager.Instance.Subscribe(GameEventNames.HEALTH_CHANGED, OnHealthChanged);
        // 订阅对话结束事件，监听Boss出现
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }

    private void OnDisable()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe(GameEventNames.HEALTH_UI_INIT, OnHealthUIInit);
        EventManager.Instance.Unsubscribe(GameEventNames.HEALTH_CHANGED, OnHealthChanged);
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }
    
    /// <summary>
    /// 处理对话结束事件，监听Boss出现
    /// </summary>
    private void OnDialogEnd(object data)
    {
        if (data is int dialogId && dialogId == bossDialogId && !isBossAppeared)
        {
            isBossAppeared = true;
            ShowHealthUI();
        }
    }
    
    /// <summary>
    /// 显示生命值UI（带淡入效果）
    /// </summary>
    private void ShowHealthUI()
    {
        StartCoroutine(FadeInHealthUI());
    }
    
    /// <summary>
    /// 淡入显示生命值UI的协程
    /// </summary>
    private IEnumerator FadeInHealthUI()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeInDuration);
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }
        
        // 确保最终状态正确
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // 如果已经有生命值数据，更新显示
        if (maxHealth > 0)
        {
            UpdateHeartsDisplay(maxHealth);
        }
    }

    /// <summary>
    /// 初始化生命值UI
    /// </summary>
    private void OnHealthUIInit(object data)
    {
        if (data is int health)
        {
            maxHealth = health;
            CreateHearts();
        }
    }

    /// <summary>
    /// 生命值变化时更新UI
    /// </summary>
    private void OnHealthChanged(object data)
    {
        if (data is int currentHealth)
        {
            UpdateHeartsDisplay(currentHealth);
        }
    }

    /// <summary>
    /// 创建爱心UI元素
    /// </summary>
    private void CreateHearts()
    {
        // 清除现有的爱心
        ClearHearts();

        // 创建新的爱心
        for (int i = 0; i < maxHealth; i++)
        {
            CreateHeart(i);
        }
    }

    /// <summary>
    /// 创建单个爱心
    /// </summary>
    private void CreateHeart(int index)
    {
        if (heartPrefab != null && heartsContainer != null)
        {
            // 实例化爱心预制体
            GameObject heartObject = Instantiate(heartPrefab, heartsContainer);
            heartObject.name = $"Heart_{index}";

            // 设置位置
            RectTransform rectTransform = heartObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 设置水平位置，根据索引和间距排列
                Vector3 position = rectTransform.localPosition;
                position.x = index * heartSpacing;
                rectTransform.localPosition = position;
            }

            // 获取Image组件
            Image heartImage = heartObject.GetComponent<Image>();
            if (heartImage != null)
            {
                // 设置初始精灵为完整爱心
                if (fullHeartSprite != null)
                {
                    heartImage.sprite = fullHeartSprite;
                }
                heartImages.Add(heartImage);
            }
            else
            {
                Debug.LogWarning($"爱心对象 {heartObject.name} 没有Image组件");
                Destroy(heartObject);
            }
        }
        else
        {
            Debug.LogError("缺少必要的组件：heartPrefab 或 heartsContainer");
        }
    }

    /// <summary>
    /// 更新爱心显示
    /// </summary>
    private void UpdateHeartsDisplay(int currentHealth)
    {
        // 确保currentHealth在有效范围内
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 如果Boss还没出现，不更新显示
        if (!isBossAppeared)
        {
            return;
        }

        // 更新每个爱心的显示状态
        for (int i = 0; i < heartImages.Count; i++)
        {
            Image heartImage = heartImages[i];
            if (heartImage != null)
            {
                if (i < currentHealth)
                {
                    // 显示完整爱心
                    heartImage.sprite = fullHeartSprite;
                    heartImage.gameObject.SetActive(true);
                }
                else
                {
                    // 显示空爱心或隐藏
                    if (emptyHeartSprite != null)
                    {
                        heartImage.sprite = emptyHeartSprite;
                    }
                    else
                    {
                        heartImage.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 清除所有爱心
    /// </summary>
    private void ClearHearts()
    {
        // 销毁所有爱心对象
        foreach (Image heartImage in heartImages)
        {
            if (heartImage != null)
            {
                Destroy(heartImage.gameObject);
            }
        }
        // 清空列表
        heartImages.Clear();
    }

    /// <summary>
    /// 设置爱心间距
    /// </summary>
    public void SetHeartSpacing(float spacing)
    {
        heartSpacing = spacing;
        // 如果已经创建了爱心，重新排列
        if (heartImages.Count > 0)
        {
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] != null)
                {
                    RectTransform rectTransform = heartImages[i].GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector3 position = rectTransform.localPosition;
                        position.x = i * heartSpacing;
                        rectTransform.localPosition = position;
                    }
                }
            }
        }
    }
}