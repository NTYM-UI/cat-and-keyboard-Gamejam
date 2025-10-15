/// <summary>
/// 游戏事件名称常量类
/// 集中管理所有游戏事件名称，避免拼写错误并方便维护
/// </summary>
public static class GameEventNames
{
    // 玩家相关事件
    public const string PLAYER_MOVE = "PlayerMove";
    public const string PLAYER_JUMP = "PlayerJump";
    public const string PLAYER_TOGGLE_REVERSE_CONTROL = "PlayerToggleReverseControl";
    // 关卡相关事件
    public const string Into_Next_Level = "IntoNextLevel";
    // 淡入淡出相关事件
    public const string FADE_IN_START = "FadeInStart";
    public const string FADE_OUT_START = "FadeOutStart";
    public const string FADE_OUT_COMPLETE = "FadeOutComplete"; // 淡出完成事件
}