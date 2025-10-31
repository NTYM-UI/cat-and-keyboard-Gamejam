using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogText : MonoBehaviour
{
    public int ID;
    private int EnterTriggerCount = 0; // 记录进入触发器的次数

    // 检测玩家是否第一次进入触发器
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EnterTriggerCount++;
            if (EnterTriggerCount == 1)
            {
                DialogStart();
            }
        }
    }

    // 发布对话开始事件
    private void DialogStart()
    {
        EventManager.Instance.Publish(GameEventNames.DIALOG_START, ID);
    }
}
