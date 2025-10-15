using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SavePoint;

namespace Player.Respawn
{
    public class PlayerRespawn : MonoBehaviour
    {
        public static PlayerRespawn playerRespawn;
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

        private void OnEnable()
        {
            EventManager.Instance.Subscribe(GameEventNames.PLAYER_RESPAWN, Respawn);
        }

        private void OnDisable()
        {
            EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_RESPAWN, Respawn);
        }

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            // 初始时加载存档
            SaveManager.instance.SaveGame(gameObject.transform.position, level);
        }

        public void Respawn(object data)
        {
            SaveData saveData = SaveManager.instance.LoadGame();
            rb.position = new Vector2(saveData.playerPosX, saveData.playerPosY);
        }
    }
}

