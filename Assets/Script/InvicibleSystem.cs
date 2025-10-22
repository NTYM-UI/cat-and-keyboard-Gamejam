using UnityEngine;
using System.Timers;

public class InvincibilitySystem : MonoBehaviour
{
    // 无敌状态变化事件
    public event System.Action<bool> OnInvincibilityStateChanged;

    private bool isInvincible = false;
    private float duration = 3.0f; // 无敌持续时间

    public void ActivateInvincibility()
    {
        if (!isInvincible)
        {
            isInvincible = true;
            OnInvincibilityStateChanged?.Invoke(true); // 触发无敌开始事件

            // 使用定时器在指定时间后结束无敌效果
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
            OnInvincibilityStateChanged?.Invoke(false); // 触发无敌结束事件
        }
    }
}