using UnityEngine;

/// <summary>
/// 呼吸效果脚本 - 实现对象的周期性放大缩小
/// 可附加到任何需要呼吸效果的游戏对象上
/// 当玩家碰到系统核心时，系统核心会消失
/// </summary>
public class BreathEffect : MonoBehaviour
{
    [Header("呼吸效果参数")]
    [Tooltip("呼吸效果的基础缩放值")]
    [SerializeField] private float baseScale = 1f;
    [Tooltip("呼吸效果的缩放振幅")]
    [SerializeField] private float scaleAmplitude = 0.1f;
    [Tooltip("呼吸效果的周期（秒）")]
    [SerializeField] private float breathPeriod = 0.1f;
    [Tooltip("是否使用平滑的EaseInOut效果")]
    [SerializeField] private bool useSmoothEasing = true;

    private float timeCounter = 0f;
    private Transform targetTransform;

    private void Awake()
    {
        targetTransform = transform;
    }

    private void Update()
    {
        // 更新时间计数器
        timeCounter += Time.deltaTime;
        
        // 确保时间计数器在合理范围内
        if (timeCounter > breathPeriod * 10f) // 防止浮点精度问题
        {
            timeCounter = timeCounter % breathPeriod;
        }

        // 计算当前缩放比例
        float currentScale = CalculateScale();
        
        // 应用缩放
        targetTransform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    /// <summary>
    /// 计算当前应该应用的缩放比例
    /// </summary>
    /// <returns>计算出的缩放比例</returns>
    private float CalculateScale()
    {
        // 基础的正弦波计算
        float sinValue = Mathf.Sin((timeCounter / breathPeriod) * Mathf.PI * 2);
        
        if (useSmoothEasing)
        {
            // 使用平滑的EaseInOut效果，使动画更加自然
            // 先将正弦值映射到[0, 1]范围，然后应用平滑函数
            float t = (sinValue + 1f) / 2f;
            float smoothT = t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            return baseScale + (smoothT * 2f - 1f) * scaleAmplitude;
        }
        else
        {
            // 直接使用正弦波
            return baseScale + sinValue * scaleAmplitude;
        }
    }

    /// <summary>
    /// 重置呼吸效果的时间计数器
    /// </summary>
    public void ResetBreathEffect()
    {
        timeCounter = 0f;
    }

    /// <summary>
    /// 立即设置呼吸效果的缩放参数
    /// </summary>
    /// <param name="newBaseScale">新的基础缩放值</param>
    /// <param name="newAmplitude">新的缩放振幅</param>
    /// <param name="newPeriod">新的呼吸周期</param>
    public void SetBreathParameters(float newBaseScale, float newAmplitude, float newPeriod)
    {
        baseScale = newBaseScale;
        scaleAmplitude = newAmplitude;
        breathPeriod = newPeriod;
    }
    
    /// <summary>
    /// 检测玩家碰撞
    /// 当玩家碰到系统核心时，销毁系统核心
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否碰撞到玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家碰到系统核心，系统核心消失");
            // 销毁当前游戏对象（系统核心）
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 如果使用的是碰撞器而不是触发器，可以使用这个方法
    /// </summary>
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 检查是否碰撞到玩家
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("玩家碰到系统核心，系统核心消失");
            // 销毁当前游戏对象（系统核心）
            Destroy(gameObject);
        }
    }
}