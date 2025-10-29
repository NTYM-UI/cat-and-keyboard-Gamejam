using UnityEngine;

/// <summary>
/// 简单的Y轴位置锁定脚本
/// </summary>
public class LockYPosition : MonoBehaviour
{
    private float lockedYPosition; // 锁定的Y坐标值
    public bool isLocked = true; // 是否锁定Y轴位置

    private void Start()
    {
        // 记录初始Y坐标作为锁定值
        lockedYPosition = transform.position.y;
    }

    private void LateUpdate()
    {
        // 如果未锁定Y轴位置，则直接返回
        if (!isLocked)
        {
            return;
        }

        // 保持对象在固定的Y坐标位置
        Vector3 currentPosition = transform.position;
        transform.position = new Vector3(currentPosition.x, lockedYPosition, currentPosition.z);
    }
}