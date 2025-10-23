/// <summary>
/// 游戏事件名称常量类
/// 集中管理所有游戏事件名称，避免拼写错误并方便维护
/// </summary>
public static class GameEventNames
{
    // 玩家相关事件
    public const string PLAYER_MOVE = "PlayerMove"; // 玩家移动事件
    public const string PLAYER_JUMP = "PlayerJump"; // 玩家跳跃事件
    public const string PLAYER_TOGGLE_REVERSE_CONTROL = "PlayerToggleReverseControl"; // 玩家切换控制方向事件
    public const string PLAYER_DEATH = "PlayerDeath"; // 玩家死亡事件
    public const string PLAYER_RESPAWN = "PlayerRespawn"; // 玩家复活事件
    
    // 存档点相关事件
    public const string CHECKPOINT_UPDATE = "CheckpointUpdate"; // 存档点更新事件

    // 关卡相关事件
    public const string Into_Next_Level = "IntoNextLevel"; // 进入下一关事件

    // 淡入淡出相关事件
    public const string FADE_IN_START = "FadeInStart"; // 淡入开始事件
    public const string FADE_OUT_START = "FadeOutStart"; // 淡出开始事件
    public const string FADE_OUT_COMPLETE = "FadeOutComplete"; // 淡出完成事件
    
    // 对话相关事件
    public const string DIALOG_START = "DialogStart"; // 开始对话事件
    public const string DIALOG_END = "DialogEnd"; // 对话结束事件
    public const string DIALOG_TYPE_SOUND = "DialogTypeSound"; // 打字音效事件
    
    // UI相关事件
    
    // 相机相关事件
    public const string CAMERA_SHAKE = "CameraShake"; // 相机震动事件
}