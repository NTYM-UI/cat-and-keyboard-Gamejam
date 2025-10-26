using UnityEngine;

/// <summary>玩家缩放控制器 - 专注于处理玩家大小的缩放功能</summary>
public class PlayerScaling : MonoBehaviour
{
    [Header("缩放设置")]
    [Tooltip("缩放平滑过渡速度")]
    public float lerpSpeed = 5f;
    
    [Tooltip("缩放完成阈值 - 当与目标缩放差异小于此值时视为完成")]
    public float scaleCompleteThreshold = 0.01f;
    
    private Vector3 initialScale; // 存储角色初始缩放大小作为基准
    private Vector3 windowScaleFactor = Vector3.one; // 窗口缩放因子
    private Vector3 pendingScaleFactor = Vector3.one; // 待处理的缩放因子（对话期间存储）
    private bool isDialogActive = false; // 对话是否激活
    
    // 存储初始检测参数
    private float initialGroundCheckRadius;
    private float initialWallCheckDistance;
    private float initialWallCheckHeightOffset;
    
    // 组件引用
    private PlayerController playerController;

    private void Start()
    {
        // 记录角色当前的缩放大小作为基准
        initialScale = transform.localScale;
        
        // 获取PlayerController组件
        playerController = GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            // 获取并存储初始检测参数
            // 使用反射获取私有字段
            System.Reflection.FieldInfo groundCheckRadiusField = typeof(PlayerController).GetField("groundCheckRadius", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo wallCheckDistanceField = typeof(PlayerController).GetField("wallCheckDistance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo wallCheckHeightOffsetField = typeof(PlayerController).GetField("wallCheckHeightOffset", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (groundCheckRadiusField != null)
                initialGroundCheckRadius = (float)groundCheckRadiusField.GetValue(playerController);
            if (wallCheckDistanceField != null)
                initialWallCheckDistance = (float)wallCheckDistanceField.GetValue(playerController);
            if (wallCheckHeightOffsetField != null)
                initialWallCheckHeightOffset = (float)wallCheckHeightOffsetField.GetValue(playerController);
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.WINDOW_SCALE_CHANGED, OnWindowScaleChanged);
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_START, OnDialogStart);
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.WINDOW_SCALE_CHANGED, OnWindowScaleChanged);
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_START, OnDialogStart);
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }
    
    /// <summary>
    /// 处理对话开始事件
    /// </summary>
    private void OnDialogStart(object data)
    {
        isDialogActive = true;
    }
    
    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    private void OnDialogEnd(object data)
    {
        isDialogActive = false;
        // 对话结束后应用待处理的缩放因子
        windowScaleFactor = pendingScaleFactor;
    }
    
    private void Update()
    {
        // 对话期间不执行缩放逻辑
        if (isDialogActive)
        {
            return;
        }
        
        // 基于初始缩放和窗口缩放因子计算目标缩放
        // 使用Vector3.Scale进行分量相乘
        Vector3 targetScale = Vector3.Scale(initialScale, windowScaleFactor);
        
        // 持续执行Lerp，确保能完成缩放过渡
        if (Vector3.Distance(transform.localScale, targetScale) > scaleCompleteThreshold)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, lerpSpeed * Time.deltaTime);
        }
        else if (Vector3.Distance(transform.localScale, targetScale) > 0f)
        {
            // 当接近目标缩放时，直接设置为目标值，避免抖动
            transform.localScale = targetScale;
        }
        
        // 同步更新检测参数以匹配角色缩放
        UpdateDetectionParameters();
    }
    
    /// <summary>
    /// 更新地面检测和墙体检测参数，使其与角色缩放同步
    /// </summary>
    private void UpdateDetectionParameters()
    {
        if (playerController == null) return;
        
        // 获取当前缩放比例（使用x轴缩放作为参考）
        float currentScaleFactor = transform.localScale.x / initialScale.x;
        
        try
        {
            // 使用反射更新私有字段
            System.Reflection.FieldInfo groundCheckRadiusField = typeof(PlayerController).GetField("groundCheckRadius", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo wallCheckDistanceField = typeof(PlayerController).GetField("wallCheckDistance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo wallCheckHeightOffsetField = typeof(PlayerController).GetField("wallCheckHeightOffset", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // 更新地面检测半径
            if (groundCheckRadiusField != null)
                groundCheckRadiusField.SetValue(playerController, initialGroundCheckRadius * currentScaleFactor);
            
            // 更新墙体检测距离
            if (wallCheckDistanceField != null)
                wallCheckDistanceField.SetValue(playerController, initialWallCheckDistance * currentScaleFactor);
            
            // 更新墙体检测高度偏移
            if (wallCheckHeightOffsetField != null)
                wallCheckHeightOffsetField.SetValue(playerController, initialWallCheckHeightOffset * currentScaleFactor);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("更新检测参数时出错: " + e.Message);
        }
    }

    /// <summary>
    /// 处理窗口缩放变化事件
    /// 当窗口缩放管理器发出缩放变化信号时，更新窗口缩放因子
    /// </summary>
    /// <param name="data">包含新缩放比例的object数据</param>
    private void OnWindowScaleChanged(object data)
    {
        // 将object类型参数转换为Vector3
        if (data is Vector3 newScaleFactor)
        {
            if (isDialogActive)
            {
                // 对话期间，只存储待处理的缩放因子
                pendingScaleFactor = newScaleFactor;
            }
            else
            {
                // 非对话期间，直接更新窗口缩放因子
                windowScaleFactor = newScaleFactor;
                pendingScaleFactor = newScaleFactor; // 同时更新待处理因子
            }
        }
    }
}