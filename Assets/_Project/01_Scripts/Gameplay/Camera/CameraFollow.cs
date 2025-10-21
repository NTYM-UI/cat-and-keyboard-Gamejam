using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing;

    void FixedUpdate()
    {
        if (target != null)
        {
            if (transform.position != target.position)
            {
                Vector3 targetPos = target.position;
                transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
            }
        }
    }
}