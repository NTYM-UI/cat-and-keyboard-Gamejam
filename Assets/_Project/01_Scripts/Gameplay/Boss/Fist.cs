using UnityEngine;
using System.Collections.Generic;

public class Fist : MonoBehaviour
{
    [SerializeField] private int damage = 1;          // 拳头造成的伤害
    [SerializeField] private float lifetime = 0.2f;      // 拳头的生命周期（秒）
    [SerializeField] private float impactForce = 1f;   // 碰撞时的冲击力
    [SerializeField] private GameObject impactEffectPrefab; // 落地特效预制体
    [SerializeField] private GameObject trailEffectPrefab;  // 拖尾特效预制体
    [SerializeField] private Transform trailPositionTransform;  // 拖尾特效位置控制子对象
    
    private Rigidbody2D rb;
    private GameObject currentTrailEffect;               // 当前实例化的拖尾特效
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 实例化拖尾特效预制体
        if (trailEffectPrefab != null)
        {
            // 如果有指定位置控制子对象，则在该子对象下实例化，否则直接作为拳头的子对象
            Transform parentTransform = trailPositionTransform != null ? trailPositionTransform : transform;
            
            currentTrailEffect = Instantiate(trailEffectPrefab, parentTransform);
            currentTrailEffect.transform.localPosition = Vector3.zero; // 相对于位置控制子对象的本地位置设为0
            currentTrailEffect.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("Fist: 未设置拖尾特效预制体，请在Inspector中分配");
        }
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
            // 禁用或移除拖尾特效
            if (currentTrailEffect != null)
            {
                // 尝试获取拖尾组件并禁用发射
                TrailRenderer trail = currentTrailEffect.GetComponentInChildren<TrailRenderer>();
                if (trail != null)
                {
                    trail.emitting = false;
                }
                
                // 可以选择让拖尾淡出而不是立即销毁
                // Destroy(currentTrailEffect, 0.5f); // 延迟销毁，让现有拖尾逐渐消失
            }
            
            // 播放落地特效
            PlayImpactEffect();
            
            // 触发屏幕震动
            CameraShakeHelper.TriggerCameraShake();
            
            // 停止拳头的移动
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic; // 设为运动学刚体，使其停止移动
            }
            
            // 延迟1秒后销毁拳头
            Destroy(gameObject);
        }
    }
    
    // 播放击中效果
    private void PlayImpactEffect()
    {
        if (impactEffectPrefab != null)
        {
            // 在拳头位置实例化特效，完全保持预制体中的原始变换（位置、旋转、缩放）
            // 但将位置设置为拳头当前位置，保留预制体中的旋转和缩放
            GameObject effect = Instantiate(impactEffectPrefab, transform.position, impactEffectPrefab.transform.rotation);
            effect.transform.localScale = impactEffectPrefab.transform.localScale;
            
            // 确保所有子对象的相对位置保持不变
            // （Unity的Instantiate函数默认会保持子对象的相对位置，但这里明确设置以确保）
            
            // 计算最长持续时间，确保所有效果都能完整播放
            float maxDuration = CalculateMaxEffectDuration(effect);
            
            // 设置特效生命周期，确保所有子对象的特效都能播放完毕
            Destroy(effect, maxDuration);
        }
    }
    
    /// <summary>
    /// 计算特效及其所有子对象的最长持续时间
    /// </summary>
    private float CalculateMaxEffectDuration(GameObject effectObject)
    {
        float maxDuration = 2f; // 默认生命周期
        
        // 获取所有粒子系统组件（包括子对象）
        ParticleSystem[] allParticleSystems = effectObject.GetComponentsInChildren<ParticleSystem>(true); // true表示包含非激活对象
        
        foreach (ParticleSystem ps in allParticleSystems)
        {
            // 计算完整的持续时间（包括起始延迟和循环时间）
            float totalDuration = ps.main.startDelay.constantMax + ps.main.duration;
            if (ps.main.loop && ps.main.duration > 0)
            {
                // 对于循环特效，我们可以设置一个合理的持续时间
                totalDuration = Mathf.Max(totalDuration, 2f);
            }
            
            if (totalDuration > maxDuration)
            {
                maxDuration = totalDuration;
            }
        }
        
        // 额外检查是否有动画组件
        Animator[] animators = effectObject.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            if (animator.runtimeAnimatorController != null)
            {
                // 获取动画控制器中的所有动画状态
                foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (clip.length > maxDuration)
                    {
                        maxDuration = clip.length;
                    }
                }
            }
        }
        
        return maxDuration;
    }
}