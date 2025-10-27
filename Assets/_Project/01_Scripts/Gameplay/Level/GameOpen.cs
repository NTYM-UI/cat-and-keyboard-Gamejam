using UnityEngine;

/// <summary>
/// 游戏退出处理器
/// 仅负责在游戏退出时恢复标准分辨率窗口
/// </summary>
public class GameOpen : MonoBehaviour
{
    /// <summary>
    /// 游戏打开时并恢复标准分辨率窗口
    /// </summary>
    void Awake()
    {
        // 发布事件恢复为标准分辨率窗口模式 
        // 添加null检查，因为在应用打开时EventManager.Instance可能返回null  
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Publish(GameEventNames.SET_GAME_WINDOW_SIZE, true);
        }
        // 无论EventManager是否可用，都确保执行日志记录
        Debug.Log("游戏打开，已尝试恢复为标准分辨率窗口模式");
    }
}