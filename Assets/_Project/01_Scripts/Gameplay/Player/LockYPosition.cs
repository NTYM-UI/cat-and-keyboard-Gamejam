using UnityEngine;

/// <summary>
/// 锁定Y轴位置脚本 - 保持游戏对象在固定的Y坐标位置
/// </summary>
public class LockYPosition : MonoBehaviour
{
    [Header("锁定设置")]
    [SerializeField] private bool isLocked = true; // 是否锁定Y轴位置
    [SerializeField] private bool useInitialY = true; // 是否使用初始Y坐标
    [SerializeField] private float customYPosition; // 自定义Y坐标值
    [SerializeField] private bool applyInLateUpdate = true; // 是否在LateUpdate中应用（可以避免与其他移动脚本冲突）
    [SerializeField] private bool debugMode = false; // 是否启用调试模式

    private float lockedYPosition; // 锁定的Y坐标值

    private void Awake()
    {
        // 初始化锁定的Y坐标
        if (useInitialY)
        {
            lockedYPosition = transform.position.y;
        }
        else
        {
            lockedYPosition = customYPosition;
        }
        
        if (debugMode)
        {
            Debug.Log($"[{gameObject.name}] Y轴锁定已初始化。锁定位置: {lockedYPosition}");
        }
    }

    private void Update()
    {
        // 根据设置选择在Update或LateUpdate中应用锁定
        if (isLocked && !applyInLateUpdate)
        {
            ApplyYLock();
        }
    }

    private void LateUpdate()
    {
        // 根据设置选择在LateUpdate中应用锁定
        if (isLocked && applyInLateUpdate)
        {
            ApplyYLock();
        }
    }

    /// <summary>
    /// 应用Y轴锁定 - 将对象位置的Y坐标设置为锁定值
    /// </summary>
    private void ApplyYLock()
    {
        Vector3 currentPosition = transform.position;
        
        // 只有当Y坐标发生变化时才更新，减少不必要的操作
        if (Mathf.Abs(currentPosition.y - lockedYPosition) > Mathf.Epsilon)
        {
            transform.position = new Vector3(currentPosition.x, lockedYPosition, currentPosition.z);
            
            if (debugMode && Mathf.Abs(currentPosition.y - lockedYPosition) > 0.1f) // 只在变化较大时打印调试信息
            {
                Debug.Log($"[{gameObject.name}] Y轴位置被修正: 从 {currentPosition.y} 到 {lockedYPosition}");
            }
        }
    }

    /// <summary>
    /// 启用或禁用Y轴锁定
    /// </summary>
    /// <param name="lockState">true: 启用锁定, false: 禁用锁定</param>
    public void SetLockState(bool lockState)
    {
        isLocked = lockState;
        if (debugMode)
        {
            Debug.Log($"[{gameObject.name}] Y轴锁定状态: {(isLocked ? "已启用" : "已禁用")}");
        }
    }

    /// <summary>
    /// 设置新的Y轴锁定位置
    /// </summary>
    /// <param name="newYPosition">新的Y坐标锁定值</param>
    public void SetLockedYPosition(float newYPosition)
    {
        lockedYPosition = newYPosition;
        useInitialY = false;
        customYPosition = newYPosition;
        
        if (debugMode)
        {
            Debug.Log($"[{gameObject.name}] Y轴锁定位置已更新为: {newYPosition}");
        }
    }

    /// <summary>
    /// 获取当前锁定的Y坐标值
    /// </summary>
    /// <returns>锁定的Y坐标值</returns>
    public float GetLockedYPosition()
    {
        return lockedYPosition;
    }

    /// <summary>
    /// 切换锁定状态
    /// </summary>
    public void ToggleLockState()
    {
        SetLockState(!isLocked);
    }

    /// <summary>
    /// 立即将对象移动到锁定的Y位置并更新锁定值
    /// </summary>
    public void ResetToCurrentYPosition()
    {
        lockedYPosition = transform.position.y;
        useInitialY = true;
        
        if (debugMode)
        {
            Debug.Log($"[{gameObject.name}] 锁定Y位置已重置为当前位置: {lockedYPosition}");
        }
    }
}