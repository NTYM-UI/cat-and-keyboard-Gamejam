using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField] private int maxHealth = 300;           // 最大生命值
    [SerializeField] private float invulnerableTime = 0.5f; // 受伤后的无敌时间
    [SerializeField] private int damageFlashCount = 3;      // 受伤闪烁次数
    
    [Header("阶段设置")]
    [SerializeField] private int phaseTwoThreshold = 200;   // 第二阶段阈值
    [SerializeField] private int phaseThreeThreshold = 100; // 第三阶段阈值

    [Header("引用设置")]
    [SerializeField] private BossAI bossAI;                 // Boss AI引用

    private int weakStateClickDamageTotal = 0;              // 虚弱状态下通过点击造成的总伤害
    private const int MAX_WEAK_STATE_CLICK_DAMAGE = 10;      // 虚弱状态下通过点击造成的最大伤害
    [SerializeField] private SpriteRenderer spriteRenderer; // 精灵渲染器
    // [SerializeField] private GameObject deathEffect;        // 死亡特效

    private int currentHealth;                              // 当前生命值
    private bool isInvulnerable = false;                    // 是否处于无敌状态
    private bool isDead = false;                            // 是否死亡
    private int currentPhase = 1;                           // 当前阶段

    private Color originalColor;                            // 原始颜色

    private void Start()
    {
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
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        if (!isClickDamage) // 非点击伤害才打印普通日志
        {
            Debug.Log($"Boss受到 {damage} 点伤害，当前生命值: {currentHealth}");
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
        
        // 通知AI进入新阶段
        if (bossAI != null)
        {
            bossAI.EnterNewPhase(currentPhase);
        }
    }
    
    // 虚弱状态现在完全由BossAI控制，不再基于血量

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

    // 获取当前生命值
    public int GetCurrentHealth()
    {
        return currentHealth;
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