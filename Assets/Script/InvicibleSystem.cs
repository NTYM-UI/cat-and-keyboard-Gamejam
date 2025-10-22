using UnityEngine;
using System.Timers;

public class InvincibilitySystem : MonoBehaviour
{
    // �޵�״̬�仯�¼�
    public event System.Action<bool> OnInvincibilityStateChanged;

    private bool isInvincible = false;
    private float duration = 3.0f; // �޵г���ʱ��

    public void ActivateInvincibility()
    {
        if (!isInvincible)
        {
            isInvincible = true;
            OnInvincibilityStateChanged?.Invoke(true); // �����޵п�ʼ�¼�

            // ʹ�ö�ʱ����ָ��ʱ�������޵�Ч��
            Timer timer = new Timer(duration * 1000);
            timer.Elapsed += (sender, e) =>
            {
                DeactivateInvincibility();
            };
            timer.AutoReset = false;
            timer.Start();
        }
    }

    public void DeactivateInvincibility()
    {
        if (isInvincible)
        {
            isInvincible = false;
            OnInvincibilityStateChanged?.Invoke(false); // �����޵н����¼�
        }
    }
}