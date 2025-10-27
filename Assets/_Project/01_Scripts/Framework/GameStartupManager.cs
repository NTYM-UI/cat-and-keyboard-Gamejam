using UnityEngine;

/// <summary>
/// 游戏启动管理器
/// 负责第三关启动时的初始化设置，包括窗口分辨率和场景加载
/// </summary>
public class GameStartupManager : MonoBehaviour
{
    [Tooltip("目标场景名称")]
    public string targetSceneName = "LevelScenes3_1";

    [Tooltip("目标分辨率宽度")]
    public int targetWidth = 1920;

    [Tooltip("目标分辨率高度")]
    public int targetHeight = 1080;

    [Tooltip("是否允许窗口调整大小")]
    public bool allowWindowResizing = true;

    private void Awake()
    {
        // 初始化游戏设置
        InitializeGameSettings();
    }

    /// <summary>
    /// 初始化游戏设置
    /// </summary>
    private void InitializeGameSettings()
    {
        // 设置窗口分辨率
        Screen.SetResolution(targetWidth, targetHeight, false); // false 表示窗口模式

        // 设置窗口可调整大小
        if (allowWindowResizing)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            // 在编辑器中，窗口大小调整由Unity编辑器控制
            // 在构建版本中，这将允许用户调整窗口大小
        }

        Debug.Log($"游戏已初始化 - 分辨率: {targetWidth}x{targetHeight}, 窗口可调整大小: {allowWindowResizing}");
    }
}