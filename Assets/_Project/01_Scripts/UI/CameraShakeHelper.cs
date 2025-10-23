using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 相机震动辅助类 - 提供简化的接口来发布震动事件
/// 示例：展示如何从外部触发相机震动
/// </summary>
public static class CameraShakeHelper
{
    /// <summary>
    /// 触发相机震动（使用默认参数）
    /// </summary>
    public static void TriggerCameraShake()
    {
        if (EventManager.Instance != null)
        {
            // 不需要传递参数，CameraShake组件会使用默认值
            EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, null);
        }
    }
    
    /// <summary>
    /// 触发相机震动（使用自定义参数）
    /// </summary>
    /// <param name="duration">震动持续时间</param>
    /// <param name="magnitude">震动幅度</param>
    public static void TriggerCameraShake(float duration, float magnitude)
    {
        if (EventManager.Instance != null)
        {
            // 创建包含自定义参数的字典
            Dictionary<string, object> shakeData = new Dictionary<string, object>
            {
                { "duration", duration },
                { "magnitude", magnitude }
            };
            
            // 发布事件
            EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, shakeData);
        }
    }
}