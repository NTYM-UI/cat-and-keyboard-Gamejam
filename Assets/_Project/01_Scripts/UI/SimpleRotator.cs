using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    public float rotateSpeed = 90f; // ��ת�ٶȣ���/�룩

    void Update()
    {
        // ��Z�ᣨ2D�е���ת�ᣩ��ת
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}