using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SavePoint;

namespace Player.Respawn
{
    public class PlayerRespawn : MonoBehaviour
    {
        public static PlayerRespawn playerRespawn=new PlayerRespawn();
        Rigidbody2D rb;
        public int level=1;

        void Awake()
        {
            if (playerRespawn == null)
            {
                playerRespawn = this; // 初始化playerRespawn
                DontDestroyOnLoad(gameObject); // 切换场景时不销毁
            }
            else
            {
                Destroy(gameObject); // 销毁重复的实例
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            SaveManager.instance.SaveGame(gameObject.transform.position, level);
        }

        public void Respawn()
        {
            SaveData saveData = SaveManager.instance.LoadGame();
            rb.position = new Vector2(saveData.playerPosX, saveData.playerPosY);
        }
    }
}

