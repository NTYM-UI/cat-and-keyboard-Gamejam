using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    public float rotateSpeed = 90f; // 旋转速度（度/秒）

    void Update()
    {
        // 绕Z轴（2D中的旋转轴）旋转
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}