using UnityEngine;

/// <summary>
/// 备用开始按钮控制器
/// 负责管理备用开始按钮的显示和隐藏，响应显示备用开始按钮事件
/// </summary>
public class BackupStartButtonController : MonoBehaviour
{
    [SerializeField]
    private GameObject backupStartButton; // 备用开始按钮对象引用

    void Start()
    {
        // 如果在Inspector中未设置引用，则尝试查找子对象
        if (backupStartButton == null)
        {
            // 使用Transform.Find查找当前对象的子对象
            Transform childTransform = transform.Find("BackupStartButton");
            if (childTransform != null)
            {
                backupStartButton = childTransform.gameObject;
                Debug.Log("已找到子对象BackupStartButton");
            }
            else
            {
                // 如果找不到子对象，尝试在整个场景中查找
                backupStartButton = GameObject.Find("BackupStartButton");
                Debug.Log(backupStartButton != null ? "在场景中找到BackupStartButton" : "未找到BackupStartButton对象");
            }
        }
    }

    private void OnEnable()
    {
        // 订阅显示备用开始按钮事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnShowBackupStartButton);
        }
        
        // 默认隐藏备用按钮
        if (backupStartButton != null)
        {
            backupStartButton.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // 取消订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnShowBackupStartButton);
        }
    }

    /// <summary>
    /// 处理显示备用开始按钮事件
    /// </summary>
    /// <param name="data">事件数据（可选）</param>
    private void OnShowBackupStartButton(object data)
    {

        if (backupStartButton != null && data is 14)
        {
            backupStartButton.SetActive(true);
            Debug.Log("备用开始按钮已显示");
        }
    }
}