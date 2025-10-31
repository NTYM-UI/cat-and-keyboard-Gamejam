using UnityEngine;

/// <summary>
/// Y轴位置锁定脚本，可响应平台断裂事件并在回到地面时重新锁定Y轴
/// </summary>
public class LockYPosition : MonoBehaviour
{
    private float lockedWorldYPosition; // 锁定的世界Y坐标值
    public bool isLocked = true; // 是否锁定Y轴位置
    private bool isOnGround = false; // 角色是否在地面上

    private void OnEnable()
    {
        // 订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe(GameEventNames.PLATFORM_BREAK, OnPlatformBreak);
            EventManager.Instance.Subscribe(GameEventNames.PLAYER_LAND, OnPlayerLand);
        }
    }

    private void OnDisable()
    {
        // 取消订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.PLATFORM_BREAK, OnPlatformBreak);
            EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_LAND, OnPlayerLand);
        }
    }

    /// <summary>
    /// 处理玩家落地事件
    /// </summary>
    private void OnPlayerLand(object data)
    {
        isLocked = true;
        isOnGround = true;
        // 不再每次落地都更新锁定位置，避免相机逐渐下移
        // 只有当平台断裂或特殊情况才更新锁定位置
    }

    private void Start()
    {
        // 记录初始世界Y坐标作为锁定值
        lockedWorldYPosition = transform.position.y;
    }

    private void LateUpdate()
    {
        // 如果未锁定Y轴位置，则直接返回
        if (!isLocked) return;

        // 保持对象在固定的世界Y坐标位置
        Vector3 currentPosition = transform.position;
        transform.position = new Vector3(currentPosition.x, lockedWorldYPosition, currentPosition.z);
    }

    /// <summary>
    /// 处理平台断裂事件
    /// </summary>
    private void OnPlatformBreak(object data)
    {
        // 当平台断裂时，取消Y轴锁定
        isLocked = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isGroundTag = collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform");
        bool isGroundLayer = collision.gameObject.layer == LayerMask.NameToLayer("Ground");

        if (isGroundTag || isGroundLayer)
        {
            isOnGround = true;

            // 只有当从非锁定状态变为锁定状态时才更新锁定位置
            if (!isLocked)
            {
                isLocked = true;
                // 不再自动更新锁定位置，保持初始高度
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        bool isGroundTag = collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform");
        bool isGroundLayer = collision.gameObject.layer == LayerMask.NameToLayer("Ground");

        if (isGroundTag || isGroundLayer)
        {
            isOnGround = false;
        }
    }
}