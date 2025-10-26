using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.Respawn;

public class PlayerDeath : MonoBehaviour
{
    private int playerDeathCount = 0;
    public int level = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            playerDeathCount++;

            if (level == 1)
            {
                if (playerDeathCount == 1)
                {
                    EventManager.Instance.Publish(GameEventNames.DIALOG_START, 23);
                }

                if (playerDeathCount == 2)
                {
                    EventManager.Instance.Publish(GameEventNames.DIALOG_START, 26);
                }

                if (playerDeathCount > 2)
                {
                    EventManager.Instance.Publish(GameEventNames.DIALOG_START, 28);
                }
            }


            // 发布玩家死亡事件
            EventManager.Instance.Publish(GameEventNames.PLAYER_DEATH);
            // 发布玩家复活事件
            EventManager.Instance.Publish(GameEventNames.PLAYER_RESPAWN);
        }
    }
}
