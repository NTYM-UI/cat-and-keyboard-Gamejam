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
    
    // 对话相关事件（会传出对应对话ID）
    public const string DIALOG_START = "DialogStart"; // 开始对话事件
    public const string ON_DIALOG = "OnDialog"; // 对话进行中事件
    public const string DIALOG_END = "DialogEnd"; // 对话结束事件
    public const string DIALOG_TYPE_SOUND = "DialogTypeSound"; // 打字音效事件
    
    // UI相关事件
    public const string HEALTH_CHANGED = "HealthChanged"; // 生命值变化事件
    public const string HEALTH_UI_INIT = "HealthUIInit"; // 生命值UI初始化事件

    // 相机相关事件
    public const string CAMERA_SHAKE = "CameraShake"; // 相机震动事件
    public const string CAMERA_SHAKE_HELPER = "CameraShakeHelper"; // 相机震动事件
    
    // 平台相关事件
    public const string PLATFORM_BREAK = "PlatformBreak"; // 平台断裂事件
    public const string PLATFORM_RECOVER = "PlatformRecover"; // 平台恢复事件
    
    // 隐藏路相关事件
    public const string HIDDEN_PATH_ACTIVATED = "HiddenPathActivated"; // 隐藏路激活事件

    // Boss相关事件
    public const string BOSS_DEFEATED = "BossDefeated"; // Boss被击败事件
    public const string BOSS_WEAPON_BUTTON_SPAWN = "BossWeaponButtonSpawn"; // 生成武器按钮事件
    public const string BOSS_WEAPON_BUTTON_PRESSED = "BossWeaponButtonPressed"; // 武器按钮被按下事件
    public const string HOLY_SPEAR_ATTACK = "HolySpearAttack"; // 圣枪攻击事件
    public const string BOSS_PHASE_CHANGE = "BossPhaseChange"; // Boss阶段变化事件
    public const string PLAYER_LAND = "PlayerLand"; // 玩家落地事件

    // 窗口缩放相关事件
    public const string WINDOW_SCALE_CHANGED = "WindowScaleChanged"; // 窗口缩放变化事件
    public const string SET_GAME_WINDOW_SIZE = "SetGameWindowSize"; // 设置游戏窗口大小事件
    
    // 音频相关事件
    public const string PLAY_MAIN_BGM = "PlayMainBGM"; // 播放主背景音乐事件
    public const string BOSS_BATTLE_START = "BossBattleStart"; // BOSS战开始播放boss音乐事件
    public const string PLAY_FIST_SOUND = "PlayFistSound"; // 播放拳头音效事件
    public const string PLAY_BOMB_SOUND = "PlayBombSound"; // 播放炸弹音效事件
    public const string PLAY_BUTTON_SOUND = "PlayButtonSound"; // 播放按钮音效事件
}