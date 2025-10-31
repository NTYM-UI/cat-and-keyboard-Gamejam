using UnityEngine;
using UnityEngine.UI;

public class StartButtonFall : MonoBehaviour
{
    public AudioSource audioSource;

    private Rigidbody2D rb;
    private bool hasShaken = false; // 用于记录是否已经震动过，避免重复震动
    private bool isInDialog = false; // 标记对话是否正在进行

    private void OnEnable()
    {
        // 订阅对话开始和结束事件
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_START, OnDialogStart);
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEnd);
    }

    private void OnDisable()
    {
        // 取消订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_START, OnDialogStart);
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEnd);
        }
    }

    void Start()
    {
        // 获取自身或父物体上的Rigidbody2D
        rb = GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            // 同时冻结X轴和Y轴
            rb.constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }

    // 当鼠标点击时调用
    private void OnMouseDown()
    {
        // 如果正在对话中，则不执行掉落逻辑
        if (isInDialog) return;
        
        if (rb != null)
        {
            // 同时解除X轴和Y轴的冻结
            rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        }
        // 播放点击音效
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    // 当碰撞开始时调用
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 确保只震动一次，并且碰撞的不是其他UI元素
        if (!hasShaken && collision.gameObject.CompareTag("CameraShake"))
        {
            // 使用基于EventManager的CameraShake系统触发震动
            EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, null);
            Debug.Log("StartButtonFall 震动");
            // 等待0.5秒后触发对话
            Invoke(nameof(TriggerDialogAfterShake), 1f);
            hasShaken = true;
        }
    }
    
    // 可以从Inspector中直接调用的公开方法
    public void TriggerShake()
    {
        // 使用基于EventManager的CameraShake系统触发震动
        EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, null);
    }
    
    /// <summary>
    /// 震动后触发对话
    /// </summary>
    private void TriggerDialogAfterShake()
    {
        EventManager.Instance.Publish(GameEventNames.DIALOG_START, 12);
    }

    /// <summary>
    /// 处理对话开始事件
    /// </summary>
    private void OnDialogStart(object data)
    {
        isInDialog = true;
    }
    
    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    private void OnDialogEnd(object data)
    {
        isInDialog = false;
    }
}