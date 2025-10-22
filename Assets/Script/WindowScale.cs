using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WindowScale : MonoBehaviour
{
    [Tooltip("基准分辨率")]
    public Vector2 baseResolution = new Vector2(1920, 1080);

    [Tooltip("默认缩放")]
    public Vector3 baseScale = Vector3.one;

    [Tooltip("缩放倍数（0-5）")]
    [Range(0, 5)] public float scaleMultiple = 1f;

    public event System.Action<Vector3> OnScaleChanged;  

    void Awake()
    {
    }

    void Update()
    {
        SetupTargetScale();
    }

    private void SetupTargetScale()
    {
        // 计算缩放因子
        float wFactor = Screen.width / baseResolution.x;
        float hFactor = Screen.height / baseResolution.y;

        // 限制缩放范围
        wFactor = Mathf.Clamp(wFactor, 1f / 1.5f, 1.5f);
        hFactor = Mathf.Clamp(hFactor, 1f / 1.5f, 1.5f);

        Vector3 desired = new Vector3(
            baseScale.x * wFactor * scaleMultiple,
            baseScale.y * hFactor * scaleMultiple,
            baseScale.z);

        // 触发事件
        OnScaleChanged?.Invoke(desired);
    }
}