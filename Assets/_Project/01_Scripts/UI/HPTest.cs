using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPTest : MonoBehaviour,LoseHP,GetHP
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            LoseTheHP();
        }
        if (collision.gameObject.CompareTag("HealingItem"))
        {
            GetTheHP();
            Destroy(collision.gameObject);
        }
    }

    public void LoseTheHP()
    {
        DisplayHP.life--;
    }

    public void GetTheHP()
    {
        DisplayHP.life++;
    }

    public void TakeDamage()
    {
        throw new System.NotImplementedException();
    }

    public void ApplyLoseHPAttribute()
    {
        throw new System.NotImplementedException();
    }

    public void ApplyGetHPAttribute()
    {
        throw new System.NotImplementedException();
    }
}
