using UnityEngine;
using System;
using System.Runtime.InteropServices;

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

    [Tooltip("是否允许窗口最大化")]
    public bool allowMaximize = false;

    // Windows API常量和函数
    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_THICKFRAME = 0x00040000; // 可调整大小的边框
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_FRAMECHANGED = 0x0020; // 强制重新绘制窗口边框

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private void Awake()
    {
        // 确保脚本在场景切换时不被销毁
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.SET_GAME_WINDOW_SIZE, InitializeGameSettings);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.SET_GAME_WINDOW_SIZE, InitializeGameSettings);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // 当应用获得焦点时，重新应用窗口限制设置
        if (hasFocus && !Screen.fullScreen)
        {
            ApplyWindowRestrictions();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // 当应用从暂停状态恢复时，重新应用窗口限制设置
        if (!pauseStatus && !Screen.fullScreen)
        {
            ApplyWindowRestrictions();
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        // 场景加载完成后，重新应用窗口限制设置
        if (!Screen.fullScreen)
        {
            ApplyWindowRestrictions();
        }
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
                
                // 设置为窗口模式
                Screen.fullScreenMode = FullScreenMode.Windowed;
                
                Debug.Log($"已设置为窗口模式 - 分辨率: {targetWidth}x{targetHeight}");
                Debug.Log($"窗口可调整大小: {allowWindowResizing}, 允许窗口最大化: {allowMaximize}");
                
                // 延迟应用窗口限制设置，确保窗口已经完全创建
                Invoke("ApplyWindowRestrictions", 0.5f);
            }

        Debug.Log($"游戏窗口设置已更新 - 窗口模式: {isWindowedMode}, 窗口可调整大小: {allowWindowResizing}");
    }

    /// <summary>
    /// 应用窗口限制设置，包括禁用最大化按钮和调整大小功能
    /// </summary>
    private void ApplyWindowRestrictions()
    {
        // 只在Windows平台执行
        #if UNITY_STANDALONE_WIN
        try
        {
            // 获取当前活动窗口句柄
            IntPtr hWnd = GetActiveWindow();
            
            if (hWnd != IntPtr.Zero)
            {
                // 获取当前窗口样式
                int currentStyle = GetWindowLong(hWnd, GWL_STYLE);
                int newStyle = currentStyle;
                
                // 根据设置决定是否禁用最大化按钮
                if (!allowMaximize)
                {
                    // 移除最大化按钮样式
                    newStyle &= ~WS_MAXIMIZEBOX;
                    Debug.Log("窗口最大化按钮已禁用");
                }
                
                // 根据设置决定是否禁用窗口调整大小
                if (!allowWindowResizing)
                {
                    // 移除可调整大小的边框样式
                    newStyle &= ~WS_THICKFRAME;
                    Debug.Log("窗口调整大小功能已禁用");
                }
                
                // 如果样式有变化，应用新样式
                if (newStyle != currentStyle)
                {
                    // 设置新的窗口样式
                    SetWindowLong(hWnd, GWL_STYLE, newStyle);
                    
                    // 刷新窗口以应用更改
                    SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                    
                    Debug.Log("窗口样式已成功更新");
                }
            }
            else
            {
                Debug.LogWarning("无法获取窗口句柄，将在稍后重试");
                // 稍后重试
                Invoke("ApplyWindowRestrictions", 0.5f);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("应用窗口限制设置时出错: " + e.Message);
        }
        #endif
    }
}