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
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // 计算衰减因子，使震动随时间逐渐减弱
                float attenuation = 1 - (elapsedTime / duration);
                float currentMagnitude = magnitude * attenuation;
                
                // 只添加震动偏移，不重置到原始位置
                // 这样相机的跟随行为会继续工作，震动效果叠加在上面
                float xOffset = Random.Range(-currentMagnitude, currentMagnitude);
                float yOffset = Random.Range(-currentMagnitude, currentMagnitude);
                
                // 应用震动偏移到当前相机位置
                Vector3 currentPosition = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(
                    currentPosition.x + xOffset,
                    currentPosition.y + yOffset,
                    currentPosition.z
                );
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 不需要恢复到原始位置，让相机继续跟随角色
        }
        
        isShaking = false;
    }
}