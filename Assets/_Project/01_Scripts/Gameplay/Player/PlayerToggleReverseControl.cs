using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToggleReverseControl : MonoBehaviour
{
    private int EnterTriggerCount = 0; // ��¼���봥�����Ĵ���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnterTriggerCount++;
            if (EnterTriggerCount == 1)
            {
                EventManager.Instance.Publish(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL);
                Debug.Log("��Ҵ���������ƿ���");
            }

        }
    }
}
