using UnityEngine;

/// <summary>
/// 玩家动画控制器 - 负责处理玩家动画状态的切换
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("动画参数设置")]
    [SerializeField] private string moveParameterName = "主角移动"; // 移动动画参数名
    [SerializeField] private string groundedParameterName = "是否着地"; // 着地状态参数名
    [SerializeField] private string jumpTriggerName = "跳跃";
    
    private Animator animator; // 动画控制器引用
    private bool isCurrentlyMoving = false; // 当前是否正在移动
    private bool isDialogActive = false; // 对话是否激活
    
    private void Awake()
    {
        // 获取动画控制器引用
        animator = GetComponent<Animator>();
    }
    
    private void OnEnable()
    {
        // 订阅玩家移动事件
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_MOVE, OnPlayerMove);
        // 订阅对话相关事件
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_START, OnDialogStart);
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }
    
    private void OnDisable()
    {
        // 取消订阅玩家移动事件
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_MOVE, OnPlayerMove);
        // 取消订阅对话相关事件
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_START, OnDialogStart);
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }
    
    /// <summary>
    /// 处理玩家移动事件
    /// </summary>
    private void OnPlayerMove(object data)
    {
        if (data is float horizontalInput && animator != null)
        {
            // 如果对话正在进行，强制设置为不移动
            if (isDialogActive)
            {
                if (isCurrentlyMoving)
                {
                    isCurrentlyMoving = false;
                    animator.SetBool(moveParameterName, false);
                }
                return;
            }
            
            // 判断是否正在移动
            bool shouldBeMoving = horizontalInput != 0;
            
            // 只有当移动状态改变时才更新动画参数
            if (shouldBeMoving != isCurrentlyMoving)
            {
                isCurrentlyMoving = shouldBeMoving;
                
                // 更新"主角移动"动画参数
                animator.SetBool(moveParameterName, isCurrentlyMoving);
                
                // 可选：添加日志以便调试
                Debug.Log($"更新移动动画参数 '{moveParameterName}': {isCurrentlyMoving}");
            }
        }
    }
    
    /// <summary>
    /// 设置着地状态
    /// </summary>
    /// <param name="isGrounded">是否着地</param>
    public void SetGrounded(bool isGrounded)
    {
        if (animator != null)
        {
            animator.SetBool(groundedParameterName, isGrounded);
        }
    }
    
    /// <summary>
    /// 触发跳跃动画
    /// </summary>
    public void TriggerJump()
    {
        if (animator != null)
        {
            animator.SetTrigger(jumpTriggerName);
        }
    }
    
    /// <summary>
    /// 强制更新移动动画状态
    /// </summary>
    /// <param name="isMoving">是否正在移动</param>
    public void ForceUpdateMoveState(bool isMoving)
    {
        isCurrentlyMoving = isMoving;
        if (animator != null)
        {
            animator.SetBool(moveParameterName, isMoving);
        }
    }
    
    /// <summary>
    /// 处理对话开始事件
    /// </summary>
    private void OnDialogStart(object data)
    {
        isDialogActive = true;
        // 对话开始时强制停止移动动画
        if (isCurrentlyMoving && animator != null)
        {
            isCurrentlyMoving = false;
            animator.SetBool(moveParameterName, false);
        }
    }
    
    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    private void OnDialogEnd(object data)
    {
        isDialogActive = false;
    }
}