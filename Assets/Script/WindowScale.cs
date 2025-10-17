using System.Collections.Generic;
using UnityEngine;

public class WindowScaler : MonoBehaviour
{
    [Tooltip("参照分辨率")]
    public Vector2 baseResolution = new Vector2(1920, 1080);

    [Tooltip("默认缩放")]
    public Vector3 baseScale = Vector3.one;

    [Tooltip("缩放倍数（0-5）")]
    [Range(0, 5)] public float scaleMultiple = 1f;

    [Tooltip("平滑速度（0-30）")]
    [Range(0, 30)] public float lerpSpeed = 12f;

    [Header("把需要缩放的对象拖进来")]
    public List<Transform> targets;

    float baseAspect;

    void Awake()
    {
        baseAspect = baseResolution.x / baseResolution.y; // 计算基准画面宽高比
        if (targets == null) targets = new List<Transform>();
    }

    void Update()
    {
        SetupTargetScale();
    }

    private void SetupTargetScale()
    {
        // 1. 先把窗口宽高分别除以基准宽高，得到独立缩放因数
        float wFactor = (float)Screen.width / baseResolution.x;
        float hFactor = (float)Screen.height / baseResolution.y;

        // 2. 乘上player的缩放倍数、默认缩放
        Vector3 desired = new Vector3(
            baseScale.x * wFactor * scaleMultiple,
            baseScale.y * hFactor * scaleMultiple,
            baseScale.z);

        // 3. 平滑过渡
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i])
                targets[i].localScale = Vector3.Lerp(targets[i].localScale, desired, lerpSpeed * Time.deltaTime);
        }
    }
}