using UnityEngine;

/// <summary>
/// 玩家输入管理器 - 负责检测玩家的键盘输入并通过EventManager发布相应的事件
/// </summary>
public class PlayerInputManager : Singleton<PlayerInputManager>
{
    // 移动输入的缓存值
    private float moveInput = 0f;
    
    // 跳跃输入的标志
    private bool jumpInput = false;

    private void Update()
    {
        HandleMoveInput();
        HandleJumpInput();
    }

    /// <summary>
    /// 处理移动输入，获取水平轴输入并发布PlayerMove事件
    /// </summary>
    private void HandleMoveInput()
    {
        moveInput = Input.GetAxis("Horizontal");
        EventManager.Instance.Publish(GameEventNames.PLAYER_MOVE, moveInput);
    }

    /// <summary>
    /// 处理跳跃输入，检测Vertical轴输入（上方向键/W键或下方向键/S键）并发布PlayerJump事件
    /// </summary>
    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)) // 检测上方向键/W键或下方向键/S键
        {
            jumpInput = true;
            EventManager.Instance.Publish(GameEventNames.PLAYER_JUMP, null);
        }
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
        {
            jumpInput = false;
        }
    }

    // 公共API方法
    public float GetMoveInput() => moveInput;
    public bool IsJumpPressed() => jumpInput;
}