using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 相机震动管理器 - 仅通过EventManager接收震动事件
/// </summary>
public class CameraShake : MonoBehaviour
{
    // 震动参数的默认值
    [Header("默认震动参数")]
    [Tooltip("默认震动持续时间")]
    public float defaultShakeDuration = 0.3f;
    [Tooltip("默认震动幅度")]
    public float defaultShakeMagnitude = 0.1f;
    
    // 是否正在震动中
    private bool isShaking = false;

    private void OnEnable()
    {
        // 订阅相机震动事件
        EventManager.Instance.Subscribe(GameEventNames.CAMERA_SHAKE, HandleCameraShakeEvent);
    }

    private void OnDisable()
    {
        // 取消订阅相机震动事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.CAMERA_SHAKE, HandleCameraShakeEvent);
        }
    }

    /// <summary>
    /// 处理相机震动事件
    /// 参数格式: Dictionary<string, object> { "duration": float, "magnitude": float }
    /// 外部可通过发布CAMERA_SHAKE事件来触发震动
    /// </summary>
    private void HandleCameraShakeEvent(object data)
    {
        // 设置默认参数
        float duration = defaultShakeDuration;
        float magnitude = defaultShakeMagnitude;

        // 尝试从事件数据中提取自定义参数
        if (data is Dictionary<string, object> shakeData)
        {
            if (shakeData.ContainsKey("duration") && shakeData["duration"] is float durationValue)
            {
                duration = durationValue;
            }
            if (shakeData.ContainsKey("magnitude") && shakeData["magnitude"] is float magnitudeValue)
            {
                magnitude = magnitudeValue;
            }
        }

        // 执行震动
        if (!isShaking && Camera.main != null)
        {
            StartCoroutine(DoShake(duration, magnitude));
        }
    }

    /// <summary>
    /// 实际执行震动的协程
    /// </summary>
    private System.Collections.IEnumerator DoShake(float duration, float magnitude)
    {
        isShaking = true;
        
        if (Camera.main != null)
        {
            Vector3 originalPosition = Camera.main.transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // 计算随机偏移量
                float x = originalPosition.x + Random.Range(-magnitude, magnitude);
                float y = originalPosition.y + Random.Range(-magnitude, magnitude);
                
                // 应用偏移量
                Camera.main.transform.localPosition = new Vector3(x, y, originalPosition.z);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 恢复相机原始位置
            Camera.main.transform.localPosition = originalPosition;
        }
        
        isShaking = false;
    }
}