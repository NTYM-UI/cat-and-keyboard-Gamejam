using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    void Start()
    {
        // ���PlayerPref�е���������
        PlayerPrefs.DeleteAll();
        Debug.Log("�����PlayerPref�е���������");

        // ��Ϊ����ģʽ
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Publish(GameEventNames.SET_GAME_WINDOW_SIZE, true);
        }

        // �˳���Ϸ
#if UNITY_EDITOR
        // �ڱ༭��������ʱ��ֹͣ����ģʽ
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ��ʵ�ʹ������˳���Ϸ
        Application.Quit();
#endif
    }
}
