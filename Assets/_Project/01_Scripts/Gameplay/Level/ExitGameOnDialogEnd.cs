using UnityEngine;

/// <summary>
/// 在指定对话ID结束时退出游戏的脚本
/// </summary>
public class ExitGameOnDialogEnd : MonoBehaviour
{
    [Tooltip("当对话ID等于此值时退出游戏")]
    public int targetDialogId = 69;

    private string currentLevel = "LevelScenes3_1"; // 第三关场景名称

    private void OnEnable()
    {
        // 订阅对话结束事件
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }

    private void OnDisable()
    {
        // 取消订阅对话结束事件
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }

    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    /// <param name="eventData">事件数据，这里是对话ID</param>
    private void OnDialogEnd(object eventData)
    {
        // 检查是否是目标对话ID
        if ((int)eventData == targetDialogId)
        {
            // 存储第三关
            PlayerPrefs.SetString("SavedLevel", currentLevel);
            Debug.Log($"游戏进度已保存到关卡：{currentLevel}");

            // 退出游戏
            #if UNITY_EDITOR
            // 在编辑器中运行时，停止播放模式
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // 在实际构建中退出游戏
            Application.Quit();
            #endif
        }
    }
}