using UnityEngine;
using System.Collections.Generic;

public class Fist : MonoBehaviour
{
    [SerializeField] private int damage = 1;          // 拳头造成的伤害
    [SerializeField] private float lifetime = 0.2f;      // 拳头的生命周期（秒）
    [SerializeField] private float impactForce = 1f;   // 碰撞时的冲击力
    
    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测是否击中玩家
        if (other.CompareTag("Player"))
        {
            // 使用SendMessage调用玩家对象上的TakeDamage方法（如果存在）
            // 这种方式不需要直接引用Health类
            other.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Debug.Log("拳头击中玩家，发送伤害消息: " + damage);
            
            // 尝试获取玩家的刚体组件以施加冲击力
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // 计算冲击力方向（从拳头到玩家的方向）
                Vector2 impactDirection = (other.transform.position - transform.position).normalized;
                // 施加冲击力
                playerRb.AddForce(impactDirection * impactForce, ForceMode2D.Impulse);
            }
            
            // 不再销毁拳头，让它继续下落
        }
        // 检测是否击中地面
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 可以在这里添加击中效果（如粒子效果、音效等）
            
            // 触发屏幕震动
            CameraShakeHelper.TriggerCameraShake();
            
            // 停止拳头的移动
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic; // 设为运动学刚体，使其停止移动
            }
            
            // 延迟1秒后销毁拳头
            Destroy(gameObject, lifetime);
        }
    }
    
    // 如果需要的话，可以添加击中效果的方法
    private void PlayImpactEffect()
    {
        // 这里可以添加粒子效果、音效等
    }
}