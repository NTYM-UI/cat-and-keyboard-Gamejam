using UnityEngine;

/// <summary>
/// 游戏窗口管理器
/// 负责游戏启动时的初始化设置，包括窗口分辨率和场景加载
/// </summary>
public class GameWindowSizeManager : MonoBehaviour
{
    [Tooltip("目标分辨率宽度")]
    public int targetWidth = 1920;

    [Tooltip("目标分辨率高度")]
    public int targetHeight = 1080;

    [Tooltip("是否允许窗口调整大小")] 
    public bool allowWindowResizing = true;

    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.SET_GAME_WINDOW_SIZE, InitializeGameSettings);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.SET_GAME_WINDOW_SIZE, InitializeGameSettings);
    }

    /// <summary>
    /// 初始化游戏设置
    /// </summary>
    private void InitializeGameSettings(object data)
    {
        bool isWindowedMode = false; // 默认全屏模式
        
        // 处理不同类型的事件数据
        if (data is bool boolValue)
        {
            isWindowedMode = boolValue; // 使用布尔值
        }

        // 设置窗口分辨率和全屏模式
        if (!isWindowedMode) // false 表示全屏模式
        {
            // 使用全屏模式，并采用玩家电脑的当前屏幕分辨率
            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, FullScreenMode.ExclusiveFullScreen);
            Debug.Log($"已设置为全屏模式 - 使用当前屏幕分辨率: {currentResolution.width}x{currentResolution.height}");
        }
        else // true 表示窗口模式
        {
            // 窗口模式使用指定的分辨率
            Screen.SetResolution(targetWidth, targetHeight, false);
            
            // 设置窗口可调整大小
            if (allowWindowResizing)
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Debug.Log("窗口调整大小已启用");
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Debug.Log("窗口调整大小已禁用");
            }
            Debug.Log($"已设置为窗口模式 - 分辨率: {targetWidth}x{targetHeight}");
        }

        Debug.Log($"游戏窗口设置已更新 - 窗口模式: {isWindowedMode}, 窗口可调整大小: {allowWindowResizing}");
    }
}