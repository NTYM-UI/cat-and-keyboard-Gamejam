using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    void Start()
    {
        // 清除PlayerPref中的所有数据
        PlayerPrefs.DeleteAll();
        Debug.Log("已清除PlayerPref中的所有数据");

        // 变为窗口模式
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Publish(GameEventNames.SET_GAME_WINDOW_SIZE, true);
        }

        // 退出游戏
#if UNITY_EDITOR
        // 在编辑器中运行时，停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在实际构建中退出游戏
        Application.Quit();
#endif
    }
}
