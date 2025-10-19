using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayHP : MonoBehaviour
{
    public GameObject life01,life02,life03,life04,life05;
    public GameObject none01, none02, none03, none04, none05;

    public static int life=5;
    private int minLife = 0;
    private int maxLife = 5;

    //下面可以添加生命值见底的音乐，还可以添加启用菜单等ui的bool开关等
    public AudioSource deathMusic;
    private bool flag = true;

    // Start is called before the first frame update
    void Start()
    {
        deathMusic = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Life();
    }

    void Life()
    {
        if (life >= 5)
        {
            life05.SetActive(true);
            life04.SetActive(true);
            life03.SetActive(true);
            life02.SetActive(true);
            life01.SetActive(true);
            none05.SetActive(false);
            none04.SetActive(false);
            none03.SetActive(false);
            none02.SetActive(false);
            none01.SetActive(false);
            CheckHP();
        }
        if (life == 4)
        {
            life05.SetActive(false);
            life04.SetActive(true);
            life03.SetActive(true);
            life02.SetActive(true);
            life01.SetActive(true);
            none05.SetActive(true);
            none04.SetActive(false);
            none03.SetActive(false);
            none02.SetActive(false);
            none01.SetActive(false);
        }
        if (life == 3)
        {
            life05.SetActive(false);
            life04.SetActive(false);
            life03.SetActive(true);
            life02.SetActive(true);
            life01.SetActive(true);
            none05.SetActive(true);
            none04.SetActive(true);
            none03.SetActive(false);
            none02.SetActive(false);
            none01.SetActive(false);
        }
        if (life == 2)
        {
            life05.SetActive(false);
            life04.SetActive(false);
            life03.SetActive(false);
            life02.SetActive(true);
            life01.SetActive(true);
            none05.SetActive(true);
            none04.SetActive(true);
            none03.SetActive(true);
            none02.SetActive(false);
            none01.SetActive(false);
        }
        if (life == 1)
        {
            life05.SetActive(false);
            life04.SetActive(false);
            life03.SetActive(false);
            life02.SetActive(false);
            life01.SetActive(true);
            none05.SetActive(true);
            none04.SetActive(true);
            none03.SetActive(true);
            none02.SetActive(true);
            none01.SetActive(false);
        }
        if (life <= 0)
        {
            life05.SetActive(false);
            life04.SetActive(false);
            life03.SetActive(false);
            life02.SetActive(false);
            life01.SetActive(false);
            none05.SetActive(true);
            none04.SetActive(true);
            none03.SetActive(true);
            none02.SetActive(true);
            none01.SetActive(true);
            CheckHP();
            if (!deathMusic.isPlaying && flag == true)
            {
                flag=false;
                deathMusic.Play();
            }
        }
    }

    void CheckHP()
    {
        life = Mathf.Clamp(life, minLife, maxLife);
    }
}
