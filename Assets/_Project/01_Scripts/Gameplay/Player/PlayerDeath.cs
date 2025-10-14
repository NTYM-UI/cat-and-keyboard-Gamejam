using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.Respawn;

public class PlayerDeath : MonoBehaviour
{
    Rigidbody2D rb;
    PlayerRespawn rp;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            PlayerRespawn.playerRespawn.Respawn();
        }
    }
}
