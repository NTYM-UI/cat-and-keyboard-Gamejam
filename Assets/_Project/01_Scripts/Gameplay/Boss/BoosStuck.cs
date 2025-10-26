using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosStuck : MonoBehaviour
{
    public GameObject Boos;
    public bool getStruck;
    SpriteRenderer sr;
    Color color;

    // Start is called before the first frame update
    void Start()
    {
        sr = Boos.GetComponent<SpriteRenderer>();
        color = new Color(256,256,256,256);
    }

    // Update is called once per frame
    void Update()
    {
        if (getStruck)
        {
            StartCoroutine(WaitBeStruck());
            getStruck = false;
        }
    }

    IEnumerator WaitBeStruck()
    {
        
        sr.material.SetColor("_Color", Color.white+color);

        yield return new WaitForSeconds(0.05f);

        sr.material.SetColor("_Color",Color.white);
    }

}
