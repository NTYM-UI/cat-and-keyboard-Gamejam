using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    void Awake()
    {
        // 订阅无敌事件
        InvincibilitySystem invSystem = FindObjectOfType<InvincibilitySystem>();
        if (invSystem != null)
        {
            invSystem.OnInvincibilityStateChanged += UpdateDamageLogic;
        }
    }

    private void UpdateDamageLogic(bool isInvincible)
    {
        // 根据无敌状态调整伤害逻辑
        if (isInvincible)
        {
            Debug.Log("无敌状态：无视所有伤害");
        }
        else
        {
            Debug.Log("正常状态：可以受到伤害");
        }
    }
}