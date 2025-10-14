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
            instance = this; // ��ʼ��instance
            DontDestroyOnLoad(gameObject); // �л�����ʱ������
        }
        else
        {
            Destroy(gameObject); // �����ظ���ʵ��
        }
    }

    //�浵
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

    //����
    public SaveData LoadGame()
    {
        SaveData saveData = new SaveData();
        //�ڶ���������Ĭ��ֵ������մ浵��
        saveData.playerPosX = PlayerPrefs.GetFloat("PlayerPosX", 0f);
        saveData.playerPosY = PlayerPrefs.GetFloat("PlayerPosY", 0f);
        saveData.currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        return saveData;
    }

    //ɾ�����ؿ���
    public void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
    }
}
