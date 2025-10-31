using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMainBGM : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.Publish(GameEventNames.PLAY_MAIN_BGM_STOP);
    }
}
