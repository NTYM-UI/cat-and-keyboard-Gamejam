using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntoNextLevel : MonoBehaviour
{
    public AudioSource audioSource;
    public string targetSceneName = "LevelScenes";

    private void OnMouseDown()
    {
        Debug.Log("进入下一关卡: " + targetSceneName);
        // 使用项目中的自定义SceneManager单例
        SceneManager.Instance.LoadScene(targetSceneName);

        // 播放点击音效
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
