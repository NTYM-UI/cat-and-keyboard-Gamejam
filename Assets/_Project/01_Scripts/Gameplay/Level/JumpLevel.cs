using UnityEngine;

public class JumpLevel : MonoBehaviour
{
    void Awake()
    {
        // 从PlayerPrefs中获取保存的关卡
        string savedLevel = PlayerPrefs.GetString("SavedLevel", "StartScene");

        // 验证是否有保存的关卡信息
        if (!string.IsNullOrEmpty(savedLevel))
        {
            Debug.Log($"正在加载保存的关卡：{savedLevel}");
            // 使用Unity静态SceneManager类加载场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(savedLevel);
        }
        else
        {
            Debug.Log("没有找到保存的关卡信息");
        }
    }
}
