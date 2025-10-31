using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankScreen : MonoBehaviour
{
    // 对话框ID
    public const int dialogID = 0;
    public GameObject blankScreen;
    
    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_START, BlankScreenShow);
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_END, BlankScreenHide);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_START, BlankScreenShow);
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, BlankScreenHide);
    }

    //显示黑屏
    private void BlankScreenShow(object obj)
    {
        if (obj is dialogID) 
        {
            blankScreen.SetActive(true);
        }
    }

    //隐藏黑屏
    private void BlankScreenHide(object obj)
    {
        blankScreen.SetActive(false);
    }
}
