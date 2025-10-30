using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAI : MonoBehaviour
{
    // 虚弱状态事件定义
    public static event System.Action OnBossEnterWeakState;
    public static event System.Action OnBossExitWeakState;
    
    [Header("基础设置")]
    [SerializeField] private float floatSpeed = 0.5f;           // 飘动速度
    [SerializeField] private float floatAmplitude = 0.5f;      // 飘动幅度 
    [SerializeField] private float attackCooldown = 3f;        // 攻击冷却时间
    [SerializeField] private float initialDelay = 3f;          // 初始延迟攻击时间
    [SerializeField] private float teleportHeight = 7f;        // 瞬移到玩家上方的高度
    [SerializeField] private float chargeTime = 2f;            // 蓄力时间
    [SerializeField] private float bombSpawnDuration = 0.8f;   // 炸弹在生成点停留的时间
    [SerializeField] private float bombThrowSpeed = 10f;       // 炸弹投掷速度
    [SerializeField] private float weakStateDuration = 5f;     // 虚弱状态持续时间
    [SerializeField] private float moveWaitTime = 1f;          // 每次移动之间的等待时间
    [SerializeField] private float confusionDuration = 5f;     // 玩家混乱持续时间
    [SerializeField] [Range(0f, 1f)] private float confusionAttackProbability = 0.3f; // 混乱技能触发概率（0-1之间）
    [SerializeField] [Range(0f, 1f)] private float fistDropAttackProbability = 0.3f; // 拳头下落技能触发概率（0-1之间）
    [SerializeField] [Range(0f, 1f)] private float bombAttackProbability = 0.4f; // 炸弹技能触发概率（0-1之间）
    [SerializeField] private int damagePerClick = 1; // 每次点击造成的伤害
    [SerializeField] private SpriteRenderer spriteRenderer; // 精灵渲染器引用
    
    [Header("拳头下落技能设置")]
    [SerializeField] private GameObject fistPrefab;            // 拳头预制体
    [SerializeField] private float fistDropSpeed = 15f;        // 拳头下落速度
    [SerializeField] private float fistSpawnInterval = 0.3f;   // 拳头生成间隔
    [SerializeField] private int fistDropCount = 5;            // 每次技能生成的拳头数量
    [SerializeField] private float fistSpawnHeight = 15f;      // 拳头生成高度
    [SerializeField] private GameObject warningIndicatorPrefab; // 预警提示预制体
    [SerializeField] private float warningDuration = 0.8f;     // 预警提示显示时间
    [SerializeField] private float groundYPosition = -3f;      // 地面Y轴位置，用于放置预警提示

    [Header("炸弹技能设置")]
    [SerializeField] private GameObject bombPrefab;            // 炸弹预制体
    [SerializeField] private Transform bombSpawnPoint;         // 炸弹生成点
    [SerializeField] private Transform targetPlayer;           // 目标玩家
    [SerializeField] private Animator bossAnimator;            // Boss动画器
    [SerializeField] private Rigidbody2D bossRigidbody;        // Boss刚体组件
    [SerializeField] private float fallSpeed = 10f;            // 掉落速度

    // 动画参数名
    private const string PARAM_OPEN_CLOAK = "OpenCloak";      // 张开斗篷参数
    private const string PARAM_CLOSE_CLOAK = "CloseCloak";    // 收起斗篷参数
    private const string PARAM_WEAK = "Weak";                 // 虚弱参数
    private const string PARAM_OUT_WEAK = "OutWeak";          // 解除虚弱参数

    // 攻击技能类型
    public enum AttackType
    { 
        Bomb,           // 炸弹技能
        Confusion,      // 混乱技能
        FistDrop        // 拳头下落技能
    }

    // AI状态
    public enum AIState
    { 
        Idle,           // 待机状态
        Floating,       // 飘动状态
        PreparingAttack, // 准备攻击
        Charging,       // 蓄力状态
        Attacking,      // 攻chu
        Weak            // 虚弱状态
    }
    
    // 位置限制
    private const float MAX_Y_POSITION = 5.31f; // Y轴最大位置限制
    private const float MIN_X_POSITION = -10.35f; // X轴最小位置限制
    private const float MAX_X_POSITION = 11.9f;  // X轴最大位置限制
    private AIState currentState = AIState.Floating;           // 当前状态
    private AttackType currentAttackType;                      // 当前攻击类型
    private float lastAttackTime;                              // 上次攻击时间
    private bool isPlayerConfused = false;                     // 玩家是否处于混乱状态
    private float confusionEndTime;                            // 混乱结束时间
    private Vector3 targetFloatPosition;                       // 目标飘动位置
    private float weakStateStartTime;                          // 虚弱状态开始时间
    private Vector3 originalPosition;                          // 原始位置（用于飘动计算）
    private int currentPhase = 1;                              // 当前阶段，默认为第一阶段
    private List<GameObject> warningIndicators = new List<GameObject>(); // 存储所有预警提示
    private bool isAttackEnabled = true; // 控制攻击是否启用

    private void Start()
    {
        // 如果没有指定玩家，则尝试查找标签为Player的对象
        if (targetPlayer == null)
        {
            targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // 初始化原始位置
        originalPosition = transform.position;
        // 确保初始位置在X轴限制范围内
        originalPosition.x = Mathf.Clamp(originalPosition.x, MIN_X_POSITION, MAX_X_POSITION);
        // 应用初始位置限制
        transform.position = originalPosition;
        lastAttackTime = Time.time + initialDelay; // 初始延迟攻击
        
        // 确保初始rotation为0
        transform.rotation = Quaternion.identity;
        
        // 初始化精灵渲染器引用
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 初始化动画器引用
        if (bossAnimator == null)
        {
            bossAnimator = GetComponent<Animator>();
        }
        
        // 订阅圣枪攻击事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe(GameEventNames.HOLY_SPEAR_ATTACK, OnHolySpearAttack);
        }
        
        // 开始AI行为协程
        StartCoroutine(AICoroutine());
    }

    private IEnumerator AICoroutine()
    {
        while (true)
        {
            switch (currentState)
            {
                case AIState.Floating:
                    yield return StartCoroutine(FloatCoroutine());
                    break;
                case AIState.PreparingAttack:
                    yield return StartCoroutine(PrepareAttackCoroutine());
                    break;
                case AIState.Charging:
                    yield return StartCoroutine(ChargeCoroutine());
                    break;
                case AIState.Attacking:
                    yield return StartCoroutine(AttackCoroutine());
                    break;
                case AIState.Weak:
                    yield return StartCoroutine(WeakStateCoroutine());
                    break;
                case AIState.Idle:
                default:
                    yield return new WaitForSeconds(0.1f);
                    break;
            }
        }
    }

    // 飘动协程
    private IEnumerator FloatCoroutine()
    {
        // 检查是否处于虚弱状态，如果是则不移动
        if (currentState == AIState.Weak)
        {
            yield break;
        }
        
        // 生成随机飘动目标位置
        float randomX = Random.Range(-3f, 3f);
        float randomY = Random.Range(-1f, 1f);
        targetFloatPosition = originalPosition + new Vector3(randomX, randomY, 0f);
        // 应用Y轴最大限制
        targetFloatPosition.y = Mathf.Min(targetFloatPosition.y, MAX_Y_POSITION);
        // 应用X轴限制
        targetFloatPosition.x = Mathf.Clamp(targetFloatPosition.x, MIN_X_POSITION, MAX_X_POSITION);

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetFloatPosition);

        // 确保rotation始终为0，防止任何旋转
        transform.rotation = Quaternion.identity;

        // 向目标位置移动
        while (elapsedTime < journeyLength / floatSpeed)
        {
            // 检查玩家混乱状态是否结束
            if (isPlayerConfused && Time.time >= confusionEndTime)
            {
                isPlayerConfused = false;
                // 恢复玩家控制
                RestorePlayerControl();
            }

            // 检查是否可以攻击
            if (targetPlayer != null)
            {
                // 虚弱状态或攻击禁用时不能攻击
                if (currentState != AIState.Weak && isAttackEnabled && Time.time >= lastAttackTime + attackCooldown)
                {
                    currentState = AIState.PreparingAttack;
                    yield break;
                }
                else if (currentState == AIState.Weak)
                {

                }
            }

            elapsedTime += Time.deltaTime;
            float fraction = Mathf.Clamp01(elapsedTime / (journeyLength / floatSpeed));
            
            // 使用缓动函数使移动更平滑
            float smoothedFraction = EaseInOut(fraction);
            
            // 基于移动进度的飘动效果，使移动更加连贯
            float floatOffset = Mathf.Sin(fraction * Mathf.PI * 2) * floatAmplitude;
            Vector3 newPosition = Vector3.Lerp(startPosition, targetFloatPosition, smoothedFraction) + new Vector3(0f, floatOffset, 0f);
            // 应用Y轴最大限制
            newPosition.y = Mathf.Min(newPosition.y, MAX_Y_POSITION);
            // 应用X轴限制
            newPosition.x = Mathf.Clamp(newPosition.x, MIN_X_POSITION, MAX_X_POSITION);
            transform.position = newPosition;
            
            // 确保在移动过程中也不会旋转
            transform.rotation = Quaternion.identity;
            
            yield return null;
        }

        // 到达目标位置后等待一段时间
        yield return new WaitForSeconds(moveWaitTime);
    }

    // 准备攻击协程
    private IEnumerator PrepareAttackCoroutine()
    {
        if (targetPlayer == null) yield break;

        // 瞬移到玩家上方，但不超过Y轴最大限制和X轴限制
        float targetY = Mathf.Min(targetPlayer.position.y + teleportHeight, MAX_Y_POSITION);
        float targetX = Mathf.Clamp(targetPlayer.position.x, MIN_X_POSITION, MAX_X_POSITION);
        Vector3 teleportPosition = new Vector3(targetX, targetY, transform.position.z);
        transform.position = teleportPosition;

        // 确保teleport后不会旋转
        transform.rotation = Quaternion.identity;

        // 更新原始位置为当前位置（用于下次飘动）
        originalPosition = transform.position;
        // 确保原始位置也在X轴限制范围内
        originalPosition.x = Mathf.Clamp(originalPosition.x, MIN_X_POSITION, MAX_X_POSITION);

        // 随机选择攻击类型：非混乱状态时有概率释放混乱技能或拳头下落技能
        if (isPlayerConfused)
        {
            // 玩家已处于混乱状态，只能释放炸弹技能
            currentAttackType = AttackType.Bomb;
        }
        else
        {
            // 随机决定攻击类型
            float randomValue = Random.value;
            
            // 归一化概率，确保总和为1
            float totalProbability = confusionAttackProbability + fistDropAttackProbability + bombAttackProbability;
            float normalizedConfusionProb = confusionAttackProbability / totalProbability;
            float normalizedFistDropProb = fistDropAttackProbability / totalProbability;
            float normalizedBombProb = bombAttackProbability / totalProbability;
            
            if (randomValue <= normalizedConfusionProb)
            {
                currentAttackType = AttackType.Confusion;
                Debug.Log("随机选择了混乱技能");
            }
            else if (randomValue <= normalizedConfusionProb + normalizedFistDropProb)
            {
                currentAttackType = AttackType.FistDrop;
                Debug.Log("随机选择了拳头下落技能");
            }
            else
            {
                currentAttackType = AttackType.Bomb;
                Debug.Log("随机选择了炸弹技能");
            }
        }

        // 播放张开斗篷动画
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger(PARAM_OPEN_CLOAK);
        }
        
        // 等待一段时间，让boss先张开斗篷展示一下再释放技能
        yield return new WaitForSeconds(0.5f);
        
        // 根据选择的技能类型执行相应操作
        if (currentAttackType == AttackType.Bomb)
        {
            // 生成炸弹
            SpawnBomb();
        }
        else if (currentAttackType == AttackType.Confusion && !isPlayerConfused)
        {
            // 执行混乱技能
            ApplyPlayerConfusion();
        }
        else if (currentAttackType == AttackType.FistDrop)
        {
            // 开始拳头下落技能的协程
            StartCoroutine(FistDropCoroutine());
        }

        // 转换到蓄力状态
        currentState = AIState.Charging;
        yield return null;
    }

    // 蓄力协程
    private IEnumerator ChargeCoroutine()
    {
        // 等待蓄力时间 - 炸弹已经在准备攻击阶段生成
        float elapsedTime = 0f;

        while (elapsedTime < chargeTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 转换到攻击状态
        currentState = AIState.Attacking;
    }

    // 攻击协程
    private IEnumerator AttackCoroutine()
    {
        // 更新上次攻击时间
        lastAttackTime = Time.time;
        
        // 攻击类型状态更新 - 随机模式下不需要计数
        
        // 释放混乱技能后，确保收起斗篷
        if (currentAttackType == AttackType.Confusion && bossAnimator != null)
        {
            bossAnimator.SetTrigger(PARAM_CLOSE_CLOAK);
        }
        // 拳头下落技能释放后，确保收起斗篷
        else if (currentAttackType == AttackType.FistDrop && bossAnimator != null)
        {
            bossAnimator.SetTrigger(PARAM_CLOSE_CLOAK);
        }

        // 延迟一小段时间，确保状态转换平滑
        yield return new WaitForSeconds(0.5f);

        // 返回到飘动状态
        currentState = AIState.Floating;
    }

    // 虚弱状态协程
    private IEnumerator WeakStateCoroutine()
    {
        weakStateStartTime = Time.time;

        // 在虚弱状态期间
        while (Time.time < weakStateStartTime + weakStateDuration)
        {
            // 可以添加一些视觉效果，比如闪烁或颜色变化
            yield return null;
        }

        // 触发虚弱状态结束事件
        OnBossExitWeakState?.Invoke();

        // 退出虚弱状态
        if (bossAnimator != null)
        {   
            bossAnimator.SetTrigger(PARAM_OUT_WEAK);
        }
        
        // 停止掉落协程（如果正在运行）
        StopCoroutine(FallToGroundCoroutine());
        
        // 确保Rigidbody保持为Kinematic类型
        if (bossRigidbody != null)
        {   
            bossRigidbody.velocity = Vector2.zero; // 清除速度
        }
        
        // 恢复到进入虚弱状态前的状态，而不是固定的Floating状态
        // 这样可以让Boss在虚弱状态结束后继续之前的行为模式
        if (previousState != AIState.Weak)
        {   
            currentState = previousState;
        }
        else
        {   
            currentState = AIState.Floating;
        }
    }
    
    /// <summary>
    /// 控制Boss从当前位置平滑掉落到地面的协程
    /// </summary>
    private IEnumerator FallToGroundCoroutine()
    {
        // 获取当前位置
        Vector3 currentPosition = transform.position;
        
        // 计算目标位置（保持X和Z不变，Y设为地面位置）
        Vector3 targetPosition = new Vector3(currentPosition.x, groundYPosition, currentPosition.z);
        
        // 计算需要移动的距离
        float distanceToGround = Mathf.Abs(currentPosition.y - groundYPosition);
        
        // 计算掉落所需时间
        float fallDuration = distanceToGround / fallSpeed;
        float elapsedTime = 0f;
        
        // 平滑移动到地面
        while (elapsedTime < fallDuration)
        {
            // 使用线性插值平滑移动
            transform.position = Vector3.Lerp(currentPosition, targetPosition, elapsedTime / fallDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保精确地到达地面位置
        transform.position = targetPosition;
    }

    // 销毁Boss创造的所有技能
    private void DestroyAllBossSkills()
    {   
        // 销毁所有炸弹
        Bomb[] bombs = FindObjectsOfType<Bomb>();
        int bombCount = bombs.Length;
        foreach (Bomb bomb in bombs)
        {
            Destroy(bomb.gameObject);
        }
        
        // 销毁所有拳头
        Fist[] fists = FindObjectsOfType<Fist>();
        int fistCount = fists.Length;
        foreach (Fist fist in fists)
        {
            Destroy(fist.gameObject);
        }
        
        // 销毁所有预警提示
        ClearWarningIndicators();
    }
    
    // 清除所有预警提示
    private void ClearWarningIndicators()
    {
        int warningCount = warningIndicators.Count;
        foreach (GameObject indicator in warningIndicators)
        {
            if (indicator != null)
            {
                // 停止可能正在运行的淡出协程
                StopCoroutine("FadeOutWarningIndicator");
                Destroy(indicator);
            }
        }
        // 清空列表
        warningIndicators.Clear();
    }

    // 旋转向量函数
    private Vector2 RotateVector(Vector2 vector, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }

    // 生成炸弹
    private void SpawnBomb()
    {
        if (bombPrefab == null || bombSpawnPoint == null || targetPlayer == null) return;

        // 根据当前阶段决定生成炸弹的数量
        int bombCount = (currentPhase >= 2) ? 3 : 1;

        // 生成指定数量的炸弹
        for (int i = 0; i < bombCount; i++)
        {
            // 为每个炸弹计算稍微不同的生成位置，避免重叠
            float offsetX = i * 0.5f - (bombCount - 1) * 0.25f; // 居中分布
            Vector3 spawnPosition = bombSpawnPoint.position + new Vector3(offsetX, 0f, 0f);
            
            // 生成炸弹
            GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
            Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();
            
            // 禁用炸弹的物理效果，让它在生成点停留
            if (bombRb != null)
            {
                bombRb.bodyType = RigidbodyType2D.Kinematic; // 设为运动学刚体，不受物理影响
                bombRb.velocity = Vector2.zero; // 确保速度为零
            }
            
            // 启动协程，延迟后投掷炸弹
            // 为每个炸弹添加一点延迟，避免同时投出
            float delay = i * 0.2f; // 每个炸弹之间间隔0.2秒
            StartCoroutine(ThrowBombAfterDelayWithOffset(bomb, delay));
        }
    }
    
    // 带偏移延迟的投掷炸弹协程
    private IEnumerator ThrowBombAfterDelayWithOffset(GameObject bomb, float delayOffset)
    {
        // 先等待偏移延迟
        if (delayOffset > 0)
        {
            yield return new WaitForSeconds(delayOffset);
        }
        
        // 确定炸弹的索引（通过计算delayOffset确定是第几个炸弹）
        int bombIndex = Mathf.FloorToInt(delayOffset / 0.2f);
        
        // 然后调用投掷逻辑，并传入炸弹索引
        yield return StartCoroutine(ThrowBombAfterDelay(bomb, bombIndex));
    }
    
    // 延迟后投掷炸弹的协程，支持不同方向
    private IEnumerator ThrowBombAfterDelay(GameObject bomb, int bombIndex = 0)
    {
        Bomb bombScript = bomb.GetComponent<Bomb>();
        
        // 首先等待炸弹显示动画完成
        float waitTime = 0f;
        while (bombScript != null && bombScript.isAppearing && waitTime < 2f) // 2秒超时保护
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        
        // 然后等待额外的停留时间
        yield return new WaitForSeconds(bombSpawnDuration);
        
        if (bomb != null && targetPlayer != null)
        {
            Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();
            // 使用之前已经定义的bombScript变量
            
            if (bombRb != null)
            {
                // 恢复炸弹的物理效果
                bombRb.bodyType = RigidbodyType2D.Dynamic;
                
                // 计算基础方向（指向玩家）
                Vector2 baseDirection = (targetPlayer.position - bomb.transform.position).normalized;
                
                Vector2 direction;
                // 在第二阶段，让炸弹朝三个不同方向投掷
                if (currentPhase >= 2)
                {
                    // 为每个炸弹计算不同的方向偏移
                    float angleOffset;
                    switch (bombIndex)
                    {
                        case 0: // 第一个炸弹：向左偏移
                            angleOffset = -30f; // 向左偏移15度
                            break;
                        case 1: // 第二个炸弹：直接朝向玩家
                            angleOffset = 0f;
                            break;
                        case 2: // 第三个炸弹：向右偏移
                            angleOffset = 30f; // 向右偏移15度
                            break;
                        default: // 默认情况：不偏移
                            angleOffset = 0f;
                            break;
                    }
                    
                    // 计算旋转后的方向
                    direction = RotateVector(baseDirection, angleOffset);
                }
                else
                {
                    // 第一阶段，炸弹直接朝向玩家
                    direction = baseDirection;
                }
                
                // 应用初速度，让炸弹按指定方向飞去
                bombRb.velocity = direction * bombThrowSpeed;
                
                // 只有最后一个炸弹被投出时才收起斗篷（在多炸弹模式下）
                if (((currentPhase >= 2 && bombIndex == 2) || currentPhase == 1) && bossAnimator != null) 
                {
                    bossAnimator.SetTrigger(PARAM_CLOSE_CLOAK);
                }
            }
            
            // 通知炸弹开始计算爆炸时间
            if (bombScript != null)
            {
                bombScript.StartExplosionTimer();
            }
        }
    }

    private AIState previousState = AIState.Idle; // 用于保存进入虚弱状态前的状态
    
    // 外部调用方法：设置Boss进入虚弱状态
    public void SetWeakState()
    {   
        // 无论当前状态如何，都可以强制进入虚弱状态
        // 这确保即使在Boss释放技能或蓄力时，圣枪攻击也能打断并使其进入虚弱状态
        // 保存之前的状态，以便在虚弱状态结束后恢复
        previousState = currentState;
        
        // 停止所有正在运行的协程，确保彻底中断当前操作
        // 这很重要，特别是对于ChargeCoroutine等可能在后台运行并修改状态的协程
        StopAllCoroutines();
        
        // 销毁Boss创造的所有技能（炸弹和拳头）
        DestroyAllBossSkills();
        
        // 直接设置当前状态为虚弱状态
        currentState = AIState.Weak;
        
        // 触发虚弱状态开始事件
        OnBossEnterWeakState?.Invoke();
        
        // 直接触发虚弱动画
        if (bossAnimator != null)
        {    
            bossAnimator.SetTrigger(PARAM_WEAK);
        }
        
        // 作为幽灵，使用代码控制掉落而不是物理重力
        if (bossRigidbody == null)
        {   
            bossRigidbody = GetComponent<Rigidbody2D>();
        }
        
        // 确保Rigidbody在正常状态下不受重力影响
        if (bossRigidbody != null)
        {   
            // 保持为Kinematic类型，但添加向下移动的逻辑
            // 在Update或协程中实现掉落
            StartCoroutine(FallToGroundCoroutine());
        }
        
        // 重置虚弱状态下的点击伤害计数器
        BossHealth bossHealth = GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.ResetWeakStateClickDamage();
        }
        
        // 重新启动AICoroutine，以确保它能正确处理WeakStateCoroutine
        StartCoroutine(AICoroutine());
    }
    
    /// <summary>
    /// 检查Boss是否处于虚弱状态
    /// 供BossHealth调用
    /// </summary>
    /// <returns>如果Boss处于虚弱状态返回true，否则返回false</returns>
    public bool IsInWeakState()
    {
        return currentState == AIState.Weak;
    }

    // 外部调用方法：获取当前AI状态
    public AIState GetCurrentState()
    {
        return currentState;
    }
    
    // 缓动函数：使移动更平滑
    private float EaseInOut(float t)
    {
        return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
    }

    // 应用玩家混乱效果
    private void ApplyPlayerConfusion()
    {        
        try
        {
            // 确保EventManager实例存在
            if (EventManager.Instance != null)
            {
                // 发布事件切换玩家控制方向
                EventManager.Instance.Publish(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, null);
            }
            else
            {
                Debug.LogError("EventManager实例不存在，无法发布混乱控制事件!");
            }
            
            // 设置混乱状态和结束时间
            isPlayerConfused = true;
            confusionEndTime = Time.time + confusionDuration;
            
            Debug.Log("玩家进入混乱状态，持续时间: " + confusionDuration + "秒");
        }
        catch (System.Exception e)
        {
            Debug.LogError("应用混乱效果时出错: " + e.Message);
        }
    }
    
    // 恢复玩家控制
    private void RestorePlayerControl()
    {        
        try
        {
            Debug.Log("开始恢复玩家控制");
            
            // 确保EventManager实例存在
            if (EventManager.Instance != null)
            {
                // 发布事件恢复玩家控制方向
                EventManager.Instance.Publish(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, null);
            }
            else
            {
                Debug.LogError("EventManager实例不存在，无法发布恢复控制事件!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("恢复玩家控制时出错: " + e.Message);
        }
    }
    

    
    /// <summary>
    /// 处理鼠标点击事件 - 虚弱状态下点击Boss扣血
    /// </summary>
    private void OnMouseDown()
    {
        // 只有在虚弱状态下点击才有效
        if (currentState == AIState.Weak)
        {
            // 获取BossHealth组件并调用其TakeDamage方法
            BossHealth bossHealth = GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(damagePerClick);
                Debug.Log("虚弱状态下点击Boss，造成伤害: " + damagePerClick);
                
                // 添加明显的闪白效果
                StartCoroutine(FlashWhiteEffect());
            }
        }
        else
        {
            Debug.Log("Boss不在虚弱状态，点击无效");
        }
    }
    
    /// <summary>
    /// 闪白效果协程 - 当虚弱状态下被点击时触发
    /// </summary>
    private IEnumerator FlashWhiteEffect()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) yield break;
        }
        
        // 保存原始颜色
        Color originalColor = spriteRenderer.color;
        
        // 闪红效果 - 快速变为红色再恢复
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.05f); // 短暂显示红色
        spriteRenderer.color = originalColor;
        
        // 可以添加额外的轻微闪烁以增强效果
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.03f);
        spriteRenderer.color = originalColor;
    }
    
    private void OnDestroy()
    {
        // 停止所有协程
        StopAllCoroutines();
        
        // 取消订阅事件
        EventManager.Instance.Unsubscribe(GameEventNames.HOLY_SPEAR_ATTACK, OnHolySpearAttack);
    }
    
    /// <summary>
    /// 处理圣枪攻击事件
    /// </summary>
    private void OnHolySpearAttack(object data)
    {
        // 检查数据类型并获取伤害值
        if (data is float damage)
        {
            // 获取BossHealth组件并调用其TakeDamage方法
            BossHealth bossHealth = GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(Mathf.RoundToInt(damage));
            }
            
            // 移除直接设置虚弱状态的代码，改为由圣枪实际碰撞时触发
            Debug.Log($"Boss受到圣枪攻击，伤害: {damage}");
        }
    }
    
    // 拳头下落技能协程
    private IEnumerator FistDropCoroutine()
    {
        if (fistPrefab == null || targetPlayer == null) yield break;
        
        // 清空之前可能存在的预警提示
        ClearWarningIndicators();
        List<Vector3> spawnPositions = new List<Vector3>();
        
        // 第一阶段：生成所有预警提示
        for (int i = 0; i < fistDropCount; i++)
        {
            // 在X轴范围内随机生成位置，扩大范围以增加覆盖区域
            float randomOffset = Random.Range(-6f, 6f);
            float spawnX = Mathf.Clamp(targetPlayer.position.x + randomOffset, MIN_X_POSITION, MAX_X_POSITION);
            
            // 设置地面上的预警提示位置
            Vector3 warningPosition = new Vector3(spawnX, groundYPosition, transform.position.z);
            
            // 保存拳头生成位置
            Vector3 spawnPosition = new Vector3(spawnX, fistSpawnHeight, transform.position.z);
            spawnPositions.Add(spawnPosition);
            
            // 生成预警提示
            if (warningIndicatorPrefab != null)
            {
                GameObject warningIndicator = Instantiate(warningIndicatorPrefab, warningPosition, Quaternion.identity);
                warningIndicators.Add(warningIndicator);
                
                // 启动预警提示的淡入效果
                StartCoroutine(FadeInWarningIndicator(warningIndicator, 0.3f));
            }
        }
        
        // 等待所有预警提示显示一段时间
        yield return new WaitForSeconds(warningDuration);
        
        // 第二阶段：生成所有拳头并销毁预警提示
        for (int i = 0; i < fistDropCount; i++)
        {
            // 生成拳头，使用预制体中设置的旋转
            GameObject fist = Instantiate(fistPrefab, spawnPositions[i], fistPrefab.transform.rotation);
            Rigidbody2D fistRb = fist.GetComponent<Rigidbody2D>();
            
            if (fistRb != null)
            {
                // 设置拳头的下落速度
                fistRb.velocity = new Vector2(0f, -fistDropSpeed);
            }
            
            // 销毁对应的预警提示（使用淡出效果）
            if (i < warningIndicators.Count && warningIndicators[i] != null)
            {
                // 启动淡出协程而不是直接销毁
                StartCoroutine(FadeOutWarningIndicator(warningIndicators[i], 0.3f));
            }
            
            
            // 如果不是最后一个拳头，等待指定的生成间隔
            if (i < fistDropCount - 1)
            {
                yield return new WaitForSeconds(fistSpawnInterval);
            }
        }
    }
    
    /// <summary>
    /// 预警提示的淡出效果协程
    /// </summary>
    private IEnumerator FadeOutWarningIndicator(GameObject indicator, float fadeDuration)
    {
        if (indicator == null) yield break;
        
        // 获取可能的渲染组件
        SpriteRenderer spriteRenderer = indicator.GetComponent<SpriteRenderer>();
        CanvasRenderer canvasRenderer = indicator.GetComponent<CanvasRenderer>();
        
        float elapsedTime = 0f;
        Color originalColor = Color.white;
        
        // 保存原始颜色（如果有SpriteRenderer）
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // 淡出过程
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            
            // 应用透明度到相应的渲染组件
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            else if (canvasRenderer != null)
            {
                canvasRenderer.SetAlpha(alpha);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终完全透明
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }
        else if (canvasRenderer != null)
        {
            canvasRenderer.SetAlpha(0f);
        }
        
        // 淡出完成后销毁对象
        Destroy(indicator);
    }

    /// <summary>
    /// 预警提示淡入效果协程
    /// </summary>
    /// <param name="warningIndicator">预警提示对象</param>
    /// <param name="fadeInDuration">淡入持续时间</param>
    private IEnumerator FadeInWarningIndicator(GameObject warningIndicator, float fadeInDuration)
    {
        if (warningIndicator == null) yield break;
        
        // 获取渲染组件（可能是SpriteRenderer或Image）
        SpriteRenderer spriteRenderer = warningIndicator.GetComponent<SpriteRenderer>();
        CanvasRenderer canvasRenderer = warningIndicator.GetComponent<CanvasRenderer>();
        
        if (spriteRenderer != null)
        {
            // 初始设置为完全透明
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
            
            // 淡入效果
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                color.a = alpha;
                spriteRenderer.color = color;
                yield return null;
            }
            
            // 确保最终完全不透明
            color.a = 1f;
            spriteRenderer.color = color;
        }
        else if (canvasRenderer != null)
        {
            // 对于UI元素使用CanvasRenderer
            canvasRenderer.SetAlpha(0f);
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                canvasRenderer.SetAlpha(alpha);
                yield return null;
            }
            
            canvasRenderer.SetAlpha(1f);
        }
    }
    
    /// <summary>
    /// 进入新阶段
    /// </summary>
    /// <param name="phaseNumber">新阶段编号</param>
    public void EnterNewPhase(int phaseNumber)
    {
        Debug.Log($"BossAI接收到新阶段通知: 第{phaseNumber}阶段");
        
        // 更新当前阶段
        currentPhase = phaseNumber;
        
        // 根据不同阶段调整Boss的行为参数
        switch (phaseNumber)
        {
            case 2:
                // 第二阶段：增加攻击频率，增强技能效果
                attackCooldown = 2.5f; // 减少攻击冷却时间
                bombAttackProbability = 0.5f; // 增加炸弹攻击概率
                fistDropCount = 7; // 增加拳头下落数量
                fistDropSpeed = 20f; // 增加拳头下落速度（从10提高到15）
                Debug.Log("Boss进入第二阶段：攻击速度加快，炸弹攻击增多，拳头数量增加，拳头下落速度加快，一次丢三个炸弹");
                break;
                
            case 3:
                // 第三阶段：进一步增强攻击，可能改变攻击模式
                attackCooldown = 2f; // 进一步减少攻击冷却时间
                confusionAttackProbability = 0.4f; // 增加混乱攻击概率
                fistDropAttackProbability = 0.4f; // 增加拳头下落概率
                bombAttackProbability = 0.2f; // 减少炸弹攻击概率
                floatSpeed = 0.8f; // 增加飘动速度
                weakStateDuration = 4f; // 减少虚弱状态持续时间
                Debug.Log("Boss进入第三阶段：攻击速度更快，混乱和拳头攻击增多，移动速度增加，虚弱时间缩短");
                break;
        }
        
        // 可以在这里触发阶段转换的视觉效果或动画
        if (bossAnimator != null)
        {
            // 根据阶段播放不同的阶段转换动画
            // 这里需要在Animator中设置相应的动画参数和状态机
            Debug.Log($"触发第{phaseNumber}阶段转换动画");
        }
    }
    
    /// <summary>
    /// 触发退出虚弱状态的动画
    /// </summary>
    public void TriggerOutWeakAnimation()
    {
        // 确保当前状态是虚弱状态
        if (currentState == AIState.Weak && bossAnimator != null)
        {
            // 触发OutWeak的trigger参数
            bossAnimator.SetTrigger(PARAM_OUT_WEAK);
            
            // 切换状态为飘动状态
            currentState = AIState.Floating;
        }
    }
    
    /// <summary>
    /// 设置Boss攻击状态
    /// </summary>
    /// <param name="enabled">是否启用攻击</param>
    public void SetAttackEnabled(bool enabled)
    {
        isAttackEnabled = enabled;
        Debug.Log($"Boss攻击状态设置为: {enabled}");
    }
}