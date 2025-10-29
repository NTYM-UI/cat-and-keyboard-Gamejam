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

    private List<Image> heartImages = new List<Image>(); // 存储所有爱心Image组件
    private int maxHealth; // 最大生命值

    private void OnEnable()
    {
        // 订阅生命值相关事件
        EventManager.Instance.Subscribe(GameEventNames.HEALTH_UI_INIT, OnHealthUIInit);
        EventManager.Instance.Subscribe(GameEventNames.HEALTH_CHANGED, OnHealthChanged);
    }

    private void OnDisable()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe(GameEventNames.HEALTH_UI_INIT, OnHealthUIInit);
        EventManager.Instance.Unsubscribe(GameEventNames.HEALTH_CHANGED, OnHealthChanged);
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