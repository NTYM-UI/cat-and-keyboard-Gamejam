using UnityEngine;

public class WindowScale : MonoBehaviour
{
    [Tooltip("基准分辨率（运行时会被设置为游戏的实际分辨率）")]
    public Vector2 baseResolution = new Vector2(1920, 1080);

    [Tooltip("默认缩放")]
    public Vector3 baseScale = Vector3.one;

    [Tooltip("缩放倍数（0-5）")]
    [Range(0, 5)] public float scaleMultiple = 1f;

    private void Start()
    {
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
        float wFactor = Screen.width / baseResolution.x;
        float hFactor = Screen.height / baseResolution.y;

        // 限制缩放范围
        wFactor = Mathf.Clamp(wFactor, 1f / 2f, 2f);
        hFactor = Mathf.Clamp(hFactor, 1f / 2f, 2f);

        Vector3 desired = new Vector3(
            baseScale.x * wFactor * scaleMultiple,
            baseScale.y * hFactor * scaleMultiple,
            baseScale.z);

        // 触发事件
        EventManager.Instance.Publish(GameEventNames.WINDOW_SCALE_CHANGED,desired);
    }
}