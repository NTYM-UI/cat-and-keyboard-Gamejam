using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.Respawn;

namespace SavePoint
{
    public class SavePoint : MonoBehaviour
    {
        private bool isActivated = false;
        public int level=1;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && !isActivated)
            {
                isActivated = true;
                SaveManager.instance.SaveGame(gameObject.transform.position, level);
            }
        }
    }

}
