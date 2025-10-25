using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 相机震动辅助类 - 提供简化的接口来发布震动事件
/// 示例：展示如何从外部触发相机震动
/// </summary>
public class CameraShakeHelper : MonoBehaviour
{
    [Header("到指定对话ID触发震动")]
    public int targetDialogID = 68;
    [Header("震动参数")]
    [Tooltip("震动持续时间")]
    [Range(0.1f, 2f)]
    public float shakeDuration = 0.5f;
    [Tooltip("震动幅度")]
    [Range(0.05f, 1f)]
    public float shakeMagnitude = 0.2f;
    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.ON_DIALOG, CameraShake);
        EventManager.Instance.Subscribe(GameEventNames.CAMERA_SHAKE_HELPER, CameraShake);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.ON_DIALOG, CameraShake);
        EventManager.Instance.Subscribe(GameEventNames.CAMERA_SHAKE_HELPER, CameraShake);
    }
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

    private void CameraShake(object data)
    {
        if ((int)data == targetDialogID)
        {
            // 创建震动参数字典
            Dictionary<string, object> shakeData = new Dictionary<string, object>
            {
                { "duration", shakeDuration },  // 震动持续时间
                { "magnitude", shakeMagnitude } // 震动幅度
            };

            // 发布相机震动事件
            EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, shakeData);
            Debug.Log($"触发相机震动: 持续时间={shakeDuration}, 幅度={shakeMagnitude}");
        }
    }
}