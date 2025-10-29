using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossHealth : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField] private int maxHealth = 300;           // 最大生命值
    [SerializeField] private float invulnerableTime = 0.5f; // 受伤后的无敌时间
    [SerializeField] private int damageFlashCount = 3;      // 受伤闪烁次数
    
    [Header("阶段设置")]
    [SerializeField] private int phaseTwoThreshold = 200;   // 第二阶段阈值
    [SerializeField] private int phaseThreeThreshold = 100; // 第三阶段阈值
    [Tooltip("玩家落地后Boss多久恢复攻击")]
    [Range(0.5f, 5f)]
    [SerializeField] private float attackResumeDelay = 2f; // 玩家落地后2秒恢复攻击

    [Header("引用设置")]
    [SerializeField] private BossAI bossAI;                 // Boss AI引用
    [SerializeField] private GameObject phaseTransitionEffect; // 阶段切换特效

    private int weakStateClickDamageTotal = 0;              // 虚弱状态下通过点击造成的总伤害
    private const int MAX_WEAK_STATE_CLICK_DAMAGE = 10;      // 虚弱状态下通过点击造成的最大伤害
    [SerializeField] private SpriteRenderer spriteRenderer; // 精灵渲染器
    // [SerializeField] private GameObject deathEffect;        // 死亡特效

    private int currentHealth;                              // 当前生命值
    private bool isInvulnerable = false;                    // 是否处于无敌状态
    private bool isDead = false;                            // 是否死亡
    private int currentPhase = 1;                           // 当前阶段
    private bool isAttackDisabled = false;                  // Boss是否被禁用攻击
    private int damageTakenInCurrentPhase = 0;              // 当前阶段已经受到的伤害
    private const int MAX_DAMAGE_PER_PHASE = 100;           // 每个阶段最多受到的伤害

    private Color originalColor;                            // 原始颜色

    private void Start()
    {
        // 订阅事件
        SubscribeToEvents();
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 获取精灵渲染器并保存原始颜色
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // 如果没有指定BossAI，尝试获取组件
        if (bossAI == null)
        {
            bossAI = GetComponent<BossAI>();
        }
    }

    // 受到伤害的方法
    public void TakeDamage(int damage)
    {
        // 如果已经死亡，不处理伤害
        if (isDead)
        {
            return;
        }
        
        // 检查是否处于虚弱状态并且是点击伤害
        bool isInWeakState = (bossAI != null && bossAI.IsInWeakState());
        bool isClickDamage = (damage == 1); // 假设点击伤害固定为1
        
        // 非虚弱状态下，检查无敌状态
        if (!isInWeakState && isInvulnerable)
        {
            return;
        }
        
        // 在虚弱状态下，如果是点击伤害，检查总伤害限制
        if (isInWeakState && isClickDamage)
        {
            // 如果总伤害已经达到上限，不处理这次伤害
            if (weakStateClickDamageTotal >= MAX_WEAK_STATE_CLICK_DAMAGE)
            {
                Debug.Log("虚弱状态下点击伤害已达到上限，本次点击无效");
                return;
            }
            
            // 计算实际能造成的伤害（不能超过剩余上限）
            int actualDamage = Mathf.Min(damage, MAX_WEAK_STATE_CLICK_DAMAGE - weakStateClickDamageTotal);
            weakStateClickDamageTotal += actualDamage;
            damage = actualDamage;
            Debug.Log($"虚弱状态下点击造成 {damage} 点伤害，累计: {weakStateClickDamageTotal}/{MAX_WEAK_STATE_CLICK_DAMAGE}");
        }
        
        // 减少生命值
        int previousHealth = currentHealth;
        
        // 计算当前阶段剩余可承受的伤害
        int remainingDamageThisPhase = MAX_DAMAGE_PER_PHASE - damageTakenInCurrentPhase;
        // 限制实际造成的伤害不超过当前阶段剩余可承受的伤害
        int finalDamage = Mathf.Min(damage, remainingDamageThisPhase);
        
        // 只有在虚弱状态或普通攻击时才增加当前阶段伤害计数
        if (!isInWeakState || !isClickDamage)
        {
            damageTakenInCurrentPhase += finalDamage;
        }
        
        // 应用实际伤害
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        
        if (!isClickDamage) // 非点击伤害才打印普通日志
        {
            Debug.Log($"Boss受到 {finalDamage} 点伤害，当前生命值: {currentHealth}");
            Debug.Log($"当前阶段已受伤害: {damageTakenInCurrentPhase}/{MAX_DAMAGE_PER_PHASE}");
        }
        
        // 播放受伤动画或效果
        StartCoroutine(DamageFlashEffect());
        
        // 只有非虚弱状态下才设置无敌状态
        if (!isInWeakState)
        {
            StartCoroutine(InvulnerableCoroutine());
        }
        
        // 检查是否需要进入下一个阶段
        CheckPhaseTransition(previousHealth);
        
        // 移除基于血量的虚弱状态检查，改为由圣枪攻击触发
        // CheckWeakState();
        
        // 检查是否死亡
        CheckDeath();
    }
    
    /// <summary>
    /// 重置虚弱状态下的点击伤害计数器
    /// 在虚弱状态开始时调用
    /// </summary>
    public void ResetWeakStateClickDamage()
    {
        weakStateClickDamageTotal = 0;
        Debug.Log("虚弱状态点击伤害计数器已重置");
    }

    // 检查是否需要进入下一个阶段
    private void CheckPhaseTransition(int previousHealth)
    {
        // 检查是否从第一阶段进入第二阶段
        if (previousHealth > phaseTwoThreshold && currentHealth <= phaseTwoThreshold && currentPhase == 1)
        {
            EnterPhase(2);
        }
        // 检查是否从第二阶段进入第三阶段
        else if (previousHealth > phaseThreeThreshold && currentHealth <= phaseThreeThreshold && currentPhase == 2)
        {
            EnterPhase(3);
        }
    }
    
    // 进入指定阶段
    private void EnterPhase(int phaseNumber)
    {
        currentPhase = phaseNumber;
        Debug.Log($"Boss进入第{currentPhase}阶段!");
        
        // 重置当前阶段已受伤害计数器
        damageTakenInCurrentPhase = 0;
        
        // 播放阶段切换特效
        PlayPhaseTransitionEffect();
        
        // 通知AI进入新阶段
        if (bossAI != null)
        {
            bossAI.EnterNewPhase(currentPhase);
        }
        
        // 禁用Boss攻击
        DisableBossAttack();
        
        // 发布Boss阶段变化事件
        EventManager.Instance.Publish(GameEventNames.BOSS_PHASE_CHANGE, currentPhase);
    }
    
    /// <summary>
    /// 禁用Boss攻击
    /// </summary>
    private void DisableBossAttack()
    {
        isAttackDisabled = true;
        if (bossAI != null)
        {
            bossAI.SetAttackEnabled(false);
        }
        Debug.Log("Boss攻击已禁用");
    }
    
    /// <summary>
    /// 启用Boss攻击
    /// </summary>
    private void EnableBossAttack()
    {
        isAttackDisabled = false;
        if (bossAI != null)
        {
            bossAI.SetAttackEnabled(true);
        }
        Debug.Log("Boss攻击已启用");
    }
    
    /// <summary>
    /// 处理平台恢复事件
    /// </summary>
    private void OnPlatformRecover(object data)
    {
        Debug.Log("平台已恢复");
        // 平台恢复后，等待玩家落地再恢复攻击
        // 这里只做日志记录，实际恢复由玩家落地事件触发
    }
    
    /// <summary>
    /// 处理玩家落地事件
    /// </summary>
    private void OnPlayerLand(object data)
    {
        if (isAttackDisabled)
        {
            Debug.Log("检测到玩家落地，延迟后恢复Boss攻击");
            // 延迟后恢复Boss攻击
            StartCoroutine(ResumeAttackAfterDelay());
        }
    }
    
    /// <summary>
    /// 延迟后恢复攻击
    /// </summary>
    private IEnumerator ResumeAttackAfterDelay()
    {
        yield return new WaitForSeconds(attackResumeDelay);
        EnableBossAttack();
    }

    // 检查是否死亡
    private void CheckDeath()
    {
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    // 死亡方法
    private void Die()
    {
        isDead = true;
        Debug.Log("Boss死亡!");
        
        //// 创建死亡特效
        //if (deathEffect != null)
        //{
        //    Instantiate(deathEffect, transform.position, Quaternion.identity);
        //}
        
        // 这里可以添加死亡相关的逻辑，比如停止AI、播放动画等
        
        // 禁用Boss对象
        gameObject.SetActive(false);
        
        // 可以触发游戏胜利事件
        EventManager.Instance.Publish(GameEventNames.BOSS_DEFEATED, null);
    }

    // 无敌状态协程
    private IEnumerator InvulnerableCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }

    // 受伤闪烁效果协程
    private IEnumerator DamageFlashEffect()
    {
        if (spriteRenderer == null) yield break;
        
        for (int i = 0; i < damageFlashCount; i++)
        {
            // 闪烁为白色
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            
            // 恢复原始颜色
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }
        
        // 确保最后是原始颜色
        spriteRenderer.color = originalColor;
    }
    
    private void SubscribeToEvents()
    {
        // 订阅平台恢复和玩家落地事件
        EventManager.Instance.Subscribe(GameEventNames.PLATFORM_RECOVER, OnPlatformRecover);
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_LAND, OnPlayerLand);
    }
    
    private void OnDisable()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe(GameEventNames.PLATFORM_RECOVER, OnPlatformRecover);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_LAND, OnPlayerLand);
    }

    // 获取当前生命值
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    [Header("特效相关设置")]
    [SerializeField] private float cameraShakeDuration = 2.0f; // 相机震动持续时间
    [SerializeField] private float cameraShakeMagnitude = 0.3f; // 相机震动幅度

    // 播放阶段切换特效
    private void PlayPhaseTransitionEffect()
    {
        if (phaseTransitionEffect != null)
        {
            GameObject effectInstance = Instantiate(phaseTransitionEffect, transform.position, Quaternion.identity);
            
            // 设置特效的父对象为Boss，以便跟随Boss移动
            effectInstance.transform.SetParent(transform);
            
            // 查找所有粒子系统组件并计算最长持续时间
            ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>();
            float maxDuration = 0f;
            
            foreach (ParticleSystem ps in particleSystems)
            {
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                if (ps.main.loop)
                {
                    duration = Mathf.Max(duration, 3f); // 为循环粒子系统设置默认最大持续时间
                }
                maxDuration = Mathf.Max(maxDuration, duration);
            }
            
            // 在播放特效时触发屏幕抖动
            TriggerCameraShake();
            
            // 延迟销毁特效对象，并在特效结束后触发地面断开
            StartCoroutine(DestroyEffectAndBreakGround(effectInstance, maxDuration));
        }
    }
    
    /// <summary>
    /// 触发相机震动
    /// </summary>
    private void TriggerCameraShake()
    {
        // 创建震动参数字典，使用Dictionary格式以匹配CameraShake组件的期望格式
        Dictionary<string, object> shakeData = new Dictionary<string, object>
        {
            { "duration", cameraShakeDuration },  // 震动持续时间
            { "magnitude", cameraShakeMagnitude } // 震动幅度
        };
        
        // 发布相机震动事件
        EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, shakeData);
        Debug.Log($"Boss阶段切换特效播放，触发屏幕抖动：持续时间 {cameraShakeDuration} 秒，幅度 {cameraShakeMagnitude}");
    }
    
    /// <summary>
    /// 延迟销毁特效并在特效播放2秒后触发地面断开
    /// </summary>
    private IEnumerator DestroyEffectAndBreakGround(GameObject effectInstance, float delay)
    {
        // 固定等待2秒，确保特效播放和相机震动有足够时间
        yield return new WaitForSeconds(2.0f);
        
        // 销毁特效对象
        Destroy(effectInstance);
        
        // 触发地面断开
        TriggerGroundBreak();
    }
    
    /// <summary>
    /// 触发地面断开
    /// </summary>
    private void TriggerGroundBreak()
    {
        // 发布地面断开事件
        EventManager.Instance.Publish(GameEventNames.PLATFORM_BREAK, null);
        Debug.Log("Boss阶段切换特效结束，触发地面断开");
    }
    
    // 获取当前阶段
    public int GetCurrentPhase()
    {
        return currentPhase;
    }

    // 获取最大生命值
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // 增加生命值（可能用于某些恢复机制）
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"Boss恢复 {amount} 点生命值，当前生命值: {currentHealth}");
    }
}