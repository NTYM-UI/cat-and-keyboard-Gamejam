using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDialog : MonoBehaviour
{
    public int StartID;

    void Start()
    {
        EventManager.Instance.Publish(GameEventNames.DIALOG_START, StartID);
    }
}
