using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this; // 初始化instance
            DontDestroyOnLoad(gameObject); // 切换场景时不销毁
        }
        else
        {
            Destroy(gameObject); // 销毁重复的实例
        }
    }

    //存档
    public void SaveGame(Vector2 playerPos,int level)
    {
        SaveData saveData = new SaveData();
        saveData.playerPosX = playerPos.x;
        saveData.playerPosY = playerPos.y;
        saveData.currentLevel = level;

        PlayerPrefs.SetFloat("PlayerPosX", saveData.playerPosX);
        PlayerPrefs.SetFloat("PlayerPosY", saveData.playerPosY);
        PlayerPrefs.SetInt("CurrentLevel", saveData.currentLevel);
        PlayerPrefs.Save();
    }

    //读档
    public SaveData LoadGame()
    {
        SaveData saveData = new SaveData();
        //第二个传参是默认值（避免空存档）
        saveData.playerPosX = PlayerPrefs.GetFloat("PlayerPosX", 0f);
        saveData.playerPosY = PlayerPrefs.GetFloat("PlayerPosY", 0f);
        saveData.currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        return saveData;
    }

    //删档（重开）
    public void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
    }
}
