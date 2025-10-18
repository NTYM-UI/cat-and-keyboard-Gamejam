using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.Publish(GameEventNames.DIALOG_START);
    }
}
