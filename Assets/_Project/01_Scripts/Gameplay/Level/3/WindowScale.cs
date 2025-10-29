using UnityEngine;
using System.Collections;

public class WindowScale : MonoBehaviour
{
    [Tooltip("基准分辨率")]
    public Vector2 baseResolution;

    [Tooltip("默认缩放")]
    public Vector3 baseScale = Vector3.one;

    [Tooltip("缩放倍数")]
    [Range(0, 10)] public float scaleMultiple = 1f;
    
    // 存储上一次的缩放值，用于检测是否发生变化
    private Vector3 lastScaleValue = Vector3.zero;

    private void Start()
    {
        // 使用协程来等待一帧
        StartCoroutine(InitializeResolutionAfterFrame());
    }
    
    private IEnumerator InitializeResolutionAfterFrame()
    {
        // 等待一帧，确保Screen.width和Screen.height已更新
        yield return null;
        // 将基准分辨率设置为游戏的实际分辨率
        baseResolution.x = Screen.width;
        baseResolution.y = Screen.height;
    }

    void Update()
    {
        SetupTargetScale();
    }

    private void SetupTargetScale()
    {
        // 计算缩放因子
        float wFactor = Mathf.Pow((Screen.width / baseResolution.x), 2);
        float hFactor = Mathf.Pow((Screen.height / baseResolution.y), 2);

        // 限制缩放范围
        // 降低最小缩放限制，提高最大缩放限制
        wFactor = Mathf.Clamp(wFactor, 0.5f, 10f);
        hFactor = Mathf.Clamp(hFactor, 0.5f, 10f);

        Vector3 desired = new Vector3(
            baseScale.x * wFactor * scaleMultiple,
            baseScale.y * hFactor * scaleMultiple,
            baseScale.z);
            
        // 只有当缩放值发生变化时才发布事件
        if (desired != lastScaleValue)
        {
            // 触发事件
            EventManager.Instance.Publish(GameEventNames.WINDOW_SCALE_CHANGED, desired);
            // 更新上一次的缩放值
            lastScaleValue = desired;
        }
    }
}