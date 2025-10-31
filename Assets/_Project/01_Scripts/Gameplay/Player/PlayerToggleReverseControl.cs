using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToggleReverseControl : MonoBehaviour
{
    private int EnterTriggerCount = 0; // 记录进入触发器的次数
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnterTriggerCount++;
            if (EnterTriggerCount == 1)
            {
                EventManager.Instance.Publish(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL);
                Debug.Log("玩家触发反向控制开关");
            }

        }
    }
}
