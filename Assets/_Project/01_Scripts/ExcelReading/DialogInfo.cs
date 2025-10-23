using UnityEngine;
using XlsWork;
using XlsWork.Dialogs;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

[Serializable]
public class DialogItem
{
    public string flag; // 标志位：#对话 / &选项 / END结束
    public int id;      // 行ID（对应表格B列）
    public string character; // 人物（C列）
    public string position;  // 位置（左/右，D列）
    public string content;   // 内容（E列）
    public int jumpId;       // 跳转ID（F列）
    public string effect;    // 效果（G列，如"好感度加@1"）
    public string target;    // 目标（H列）
}

public class DialogInfo : MonoBehaviour
{
    private Dictionary<int, DialogItem> dialogDict;
    private int currentDialogId; // 当前对话ID
    public int StartDialogId = 0; // 起始对话ID
    public int? EndDialogId = null; // 结束对话ID，为null时使用END标志结束

    // 引用UI元素（需在Inspector中赋值）
    public GameObject dialogPanel; // 对话面板
    public TMPro.TextMeshProUGUI speakerText; // 说话人名称
    public TMPro.TextMeshProUGUI contentText; // 对话内容
    public Transform optionParent1; // 选项按钮父物体1
    public Transform optionParent2; // 选项按钮父物体2
    public GameObject optionButtonPrefab; // 选项按钮预制体
    
    // 打字机效果相关
    [Range(0.01f, 0.1f)]
    public float typeSpeed = 0.03f; // 打字速度，越小越快
    private bool isTyping = false; // 是否正在打字
    private string fullText; // 完整的对话内容
    private Coroutine typewriterCoroutine; // 当前运行的打字机协程引用
    private UnityEngine.UI.Button panelButton; // 对话面板按钮

    // 立绘相关
    public Transform leftCharacterParent; // 左侧立绘父物体
    public Transform rightCharacterParent; // 右侧立绘父物体
    public GameObject characterPrefab; // 角色立绘预制体（包含Image和CanvasGroup）
    public float fadeDuration = 0.5f; // 立绘淡入淡出时间

    // 角色配置
    [System.Serializable]
    public class CharacterConfig
    {
        public string characterName; // 角色名称（与Excel中C列匹配）
        public Sprite normalSprite;  // 正常状态立绘
        public Sprite sadSprite;     // 悲伤状态立绘
        public Sprite angrySprite;   // 生气状态立绘
        public Sprite specialSprite; // 特殊状态立绘
    }

    public CharacterConfig[] characterConfigs; // 角色配置数组

    private Dictionary<string, CharacterConfig> characterDictionary; // 角色名称到配置的映射

    // 当前显示的立绘实例
    private GameObject leftCharacterInstance;
    private GameObject rightCharacterInstance;

    private void Awake()
    {
        // 初始化角色字典
        characterDictionary = new Dictionary<string, CharacterConfig>();
        foreach (var config in characterConfigs)
        {
            characterDictionary[config.characterName] = config;
        }

        // 初始化：加载对话表格
        dialogDict = DialogXls.LoadDialogAsDictionary();
    }

    private void OnEnable()
    {
        // 订阅对话开始事件
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_START, OnDialogStartEvent);
        // 订阅对话结束事件
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEndEvent);
    }

    private void OnDisable()
    {
        // 取消订阅对话开始事件
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_START, OnDialogStartEvent);
        // 取消订阅对话结束事件
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEndEvent);
    }

    // 处理对话开始事件
    private void OnDialogStartEvent(object eventData)
    {
        // 如果没有指定对话ID，则使用默认的起始ID
        int dialogId = StartDialogId;
        
        // 如果事件数据是整数，则使用它作为起始ID
        if (eventData is int)
        {
            dialogId = (int)eventData;
        }
        // 如果事件数据是字符串，尝试转换为整数
        else if (eventData is string strData && int.TryParse(strData, out int parsedId))
        {
            dialogId = parsedId;
        }
        
        Debug.Log($"收到对话开始事件，开始ID: {dialogId}");
        StartDialog(dialogId);
    }

    // 开始对话（传入起始ID）
    public void StartDialog(int startId)
    {
        // 添加点击事件监听
        panelButton = dialogPanel.GetComponent<UnityEngine.UI.Button>();
        if (panelButton == null)
            panelButton = dialogPanel.AddComponent<UnityEngine.UI.Button>();
        panelButton.onClick.AddListener(OnDialogPanelClick);
        panelButton.transition = UnityEngine.UI.Selectable.Transition.None;

        currentDialogId = startId;
        ShowCurrentDialog();
    }

    // 显示当前ID对应的对话内容
    private void ShowCurrentDialog()
    {
        if (!dialogDict.ContainsKey(currentDialogId))
        {
            Debug.LogError("对话ID不存在：" + currentDialogId);
            return;
        }

        // 检查是否到达指定的结束ID
        if (EndDialogId.HasValue && currentDialogId == EndDialogId.Value)
        {
            dialogPanel.SetActive(false);
            Debug.Log("到达指定的结束对话ID，对话结束：" + currentDialogId);
            // 发布对话结束事件
            EventManager.Instance.Publish(GameEventNames.DIALOG_END, currentDialogId);
            return;
        }

        DialogItem current = dialogDict[currentDialogId];
        dialogPanel.SetActive(true); // 显示对话面板

        switch (current.flag)
        {
            case "#": // 普通对话（等待点击）
                speakerText.text = current.character;
                fullText = current.content; // 保存完整文本
                contentText.text = ""; // 清空当前显示
                
                // 开始打字机效果
                if (typewriterCoroutine != null)
                    StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = StartCoroutine(TypewriterText());
                
                // 更新立绘显示
                UpdateCharacterImages(current);
                break;

            case "&": // 选项（等待玩家选择）
                ShowOptionsGroup(current.id); // 显示同组选项
                break;

            case "END": // 对话结束
                dialogPanel.SetActive(false);
                Debug.Log("对话结束ID为" + currentDialogId);
                // 发布对话结束事件
                EventManager.Instance.Publish(GameEventNames.DIALOG_END, currentDialogId);
                break;
        }
    }

    // 结束对话
    private void OnDialogEndEvent(object eventData)
    {
        if (panelButton != null)
        {
            // 移除面板点击监听
            panelButton.onClick.RemoveListener(OnDialogPanelClick);
        }
    }

    /// <summary>
    /// 更新角色立绘显示
    /// </summary>
    /// <param name="dialog">当前对话项</param>
    private void UpdateCharacterImages(DialogItem dialog)
    {
        // 提取基础角色名称（去除括号内容）
        string baseCharacterName = ExtractBaseCharacterName(dialog.character);

        // 根据位置显示对应立绘
        if (dialog.position == "左")
        {
            ShowCharacterOnLeft(baseCharacterName, dialog.content);
            HideCharacterOnRight();
        }
        else if (dialog.position == "右")
        {
            ShowCharacterOnRight(baseCharacterName, dialog.content);
            HideCharacterOnLeft();
        }
        else // 无位置信息
        {
            // 特殊处理：内心独白显示主角在左侧
            if (dialog.character.Contains("内心"))
            {
                ShowCharacterOnLeft("主角", dialog.content);
                HideCharacterOnRight();
            }
            else
            {
                // 其他情况（如旁白）隐藏立绘
                HideCharacterOnLeft();
                HideCharacterOnRight();
            }
        }
    }

    /// <summary>
    /// 提取基础角色名称（去除括号内容）
    /// </summary>
    private string ExtractBaseCharacterName(string fullName)
    {
        int index = fullName.IndexOf('（');
        if (index > 0)
        {
            return fullName.Substring(0, index);
        }
        return fullName;
    }

    /// <summary>
    /// 在左侧显示角色
    /// </summary>
    private void ShowCharacterOnLeft(string characterName, string content)
    {
        if (characterDictionary.TryGetValue(characterName, out CharacterConfig config))
        {
            // 根据内容确定表情
            Sprite sprite = DetermineExpressionSprite(config, content);

            // 如果已有实例，更新精灵；否则创建新实例
            if (leftCharacterInstance == null)
            {
                leftCharacterInstance = Instantiate(characterPrefab, leftCharacterParent);
                leftCharacterInstance.name = characterName + "_Left";
            }

            // 获取组件引用
            Image image = leftCharacterInstance.GetComponent<Image>();
            CanvasGroup canvasGroup = leftCharacterInstance.GetComponent<CanvasGroup>();

            // 设置精灵并显示
            image.sprite = sprite;
            image.preserveAspect = true;
            leftCharacterInstance.SetActive(true);

            // 淡入效果
            StartCoroutine(FadeIn(canvasGroup, fadeDuration));
        }
        else
        {
            Debug.LogWarning($"未找到角色配置: {characterName}");
            HideCharacterOnLeft();
        }
    }

    /// <summary>
    /// 在右侧显示角色
    /// </summary>
    private void ShowCharacterOnRight(string characterName, string content)
    {
        if (characterDictionary.TryGetValue(characterName, out CharacterConfig config))
        {
            // 根据内容确定表情
            Sprite sprite = DetermineExpressionSprite(config, content);

            // 如果已有实例，更新精灵；否则创建新实例
            if (rightCharacterInstance == null)
            {
                rightCharacterInstance = Instantiate(characterPrefab, rightCharacterParent);
                rightCharacterInstance.name = characterName + "_Right";
            }

            // 获取组件引用
            Image image = rightCharacterInstance.GetComponent<Image>();
            CanvasGroup canvasGroup = rightCharacterInstance.GetComponent<CanvasGroup>();

            // 设置精灵并显示
            image.sprite = sprite;
            image.preserveAspect = true;
            rightCharacterInstance.SetActive(true);

            // 淡入效果
            StartCoroutine(FadeIn(canvasGroup, fadeDuration));
        }
        else
        {
            Debug.LogWarning($"未找到角色配置: {characterName}");
            HideCharacterOnRight();
        }
    }

    /// <summary>
    /// 根据对话内容确定表情精灵
    /// </summary>
    private Sprite DetermineExpressionSprite(CharacterConfig config, string content)
    {
        // 根据关键词匹配表情
        if (content.Contains("害怕") || content.Contains("颤抖") || content.Contains("哭"))
        {
            return config.sadSprite ?? config.normalSprite;
        }
        else if (content.Contains("生气") || content.Contains("愤怒") || content.Contains("可恶"))
        {
            return config.angrySprite ?? config.normalSprite;
        }
        else if (content.Contains("特殊") || content.Contains("关键"))
        {
            return config.specialSprite ?? config.normalSprite;
        }

        // 默认使用正常表情
        return config.normalSprite;
    }

    /// <summary>
    /// 隐藏左侧角色
    /// </summary>
    private void HideCharacterOnLeft()
    {
        if (leftCharacterInstance != null && leftCharacterInstance.activeSelf)
        {
            CanvasGroup canvasGroup = leftCharacterInstance.GetComponent<CanvasGroup>();
            StartCoroutine(FadeOut(canvasGroup, fadeDuration, () => {
                leftCharacterInstance.SetActive(false);
            }));
        }
    }

    /// <summary>
    /// 隐藏右侧角色
    /// </summary>
    private void HideCharacterOnRight()
    {
        if (rightCharacterInstance != null && rightCharacterInstance.activeSelf)
        {
            CanvasGroup canvasGroup = rightCharacterInstance.GetComponent<CanvasGroup>();
            StartCoroutine(FadeOut(canvasGroup, fadeDuration, () => {
                rightCharacterInstance.SetActive(false);
            }));
        }
    }

    // 淡入协程
    private System.Collections.IEnumerator FadeIn(CanvasGroup group, float duration)
    {
        group.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        group.alpha = 1;
    }

    // 淡出协程
    private System.Collections.IEnumerator FadeOut(CanvasGroup group, float duration, Action onComplete = null)
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(startAlpha, 0, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        group.alpha = 0;
        onComplete?.Invoke();
    }

    // 显示选项组（使用两个父物体）
    private void ShowOptionsGroup(int groupId)
    {
        // 1. 清空现有选项
        ClearOptions();

        // 2. 收集所有同组选项
        List<DialogItem> options = new List<DialogItem>();
        foreach (var item in dialogDict.Values)
        {
            if (item.flag == "&" && (item.id == groupId || item.id == groupId + 1))
            {
                options.Add(item);
            }
        }

        // 3. 创建选项按钮（第一个在左，第二个在右）
        for (int i = 0; i < options.Count; i++)
        {
            Transform parent = i == 0 ? optionParent1 : optionParent2;
            CreateOptionButton(options[i], parent);
        }
    }

    // 创建单个选项按钮
    private void CreateOptionButton(DialogItem option, Transform parent)
    {
        GameObject btn = Instantiate(optionButtonPrefab, parent);
        btn.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = option.content;

        // 设置点击事件 - 关键修改：使用选项自身的ID
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            // 选择选项后跳转到该选项的ID
            currentDialogId = option.jumpId;

            // 清除选项按钮
            ClearOptions();

            // 显示下一个对话
            ShowCurrentDialog();
        });
    }

    // 清除所有选项按钮
    private void ClearOptions()
    {
        ClearParent(optionParent1);
        ClearParent(optionParent2);
    }

    // 清除指定父物体的子对象
    private void ClearParent(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    // 对话框点击事件处理
    private void OnDialogPanelClick()
    {
        DialogItem current = dialogDict[currentDialogId];

        // 只有普通对话(#)才响应点击
        if (current.flag == "#")
        {
            // 如果正在打字，则直接显示完整文本并停止打字
            if (isTyping)
            {
                if (typewriterCoroutine != null)
                {
                    StopCoroutine(typewriterCoroutine);
                    typewriterCoroutine = null;
                }
                contentText.text = fullText;
                isTyping = false;
            }
            else
            {
                // 如果已经打完字，则跳转到下一个对话
                currentDialogId = current.jumpId;
                ShowCurrentDialog();
            }
        }
    }
    
    /// <summary>
    /// 打字机效果协程
    /// </summary>
    private System.Collections.IEnumerator TypewriterText()
    {
        isTyping = true;
        contentText.text = "";
        
        // 逐字显示文本
        for (int i = 0; i < fullText.Length; i++)
        {
            contentText.text += fullText[i];
            
            // 跳过空格和特殊字符，不播放音效
            char currentChar = fullText[i];
            if (!char.IsWhiteSpace(currentChar))
            {
                // 发布打字音效事件，可以传递字符信息供音效系统使用
                EventManager.Instance.Publish(GameEventNames.DIALOG_TYPE_SOUND, currentChar);
            }
            
            yield return new WaitForSeconds(typeSpeed);
        }
        
        isTyping = false;
    }
}