using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    void Awake()
    {
        // �����޵��¼�
        InvincibilitySystem invSystem = FindObjectOfType<InvincibilitySystem>();
        if (invSystem != null)
        {
            invSystem.OnInvincibilityStateChanged += UpdateDamageLogic;
        }
    }

    private void UpdateDamageLogic(bool isInvincible)
    {
        // �����޵�״̬�����˺��߼�
        if (isInvincible)
        {
            Debug.Log("�޵�״̬�����������˺�");
        }
        else
        {
            Debug.Log("����״̬�������ܵ��˺�");
        }
    }
}