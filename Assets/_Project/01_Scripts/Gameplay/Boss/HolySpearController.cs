using UnityEngine;
using System.Collections;

/// <summary>
/// 圣枪控制器
/// 处理圣枪的飞行动画和攻击效果
/// </summary>
public class HolySpearController : MonoBehaviour
{
    [Header("飞行设置")]
    [SerializeField] private float initialSpeed = 10f; // 初始飞行速度
    [SerializeField] private float acceleration = 5f; // 加速度
    [SerializeField] private float maxSpeed = 30f; // 最大速度
    [SerializeField] private float rotationSpeed = 180f; // 旋转速度
    [SerializeField] private float flightDuration = 3f; // 飞行持续时间
    [SerializeField] private float delayBeforeAttack = 0.5f; // 攻击前延迟
    [SerializeField] private float spinningSpeed = 360f; // 自主旋转速度（度/秒）
    [SerializeField] private bool alwaysSpin = true; // 是否始终旋转，即使在追踪目标时

    [Header("视觉效果")]
    [SerializeField] private GameObject trailEffectPrefab; // 拖尾效果预制体
    [SerializeField] private GameObject hitEffectPrefab; // 击中效果预制体
    [SerializeField] private float scaleUpDuration = 0.2f; // 初始放大动画持续时间
    [SerializeField] private SpriteRenderer spearRenderer; // 圣枪的精灵渲染器
    [SerializeField] private Color startColor = new Color(1f, 0.8f, 0.2f, 1f); // 起始颜色（金色）
    [SerializeField] private Color endColor = new Color(1f, 1f, 1f, 1f); // 结束颜色（白色）

    [Header("音效")]
    [SerializeField] private AudioClip flySound; // 飞行音效
    [SerializeField] private AudioClip hitSound; // 击中音效

    [Header("伤害设置")]
    [SerializeField] private int damageAmount = 90; // 伤害值
    [SerializeField] private float damageRadius = 3f; // 伤害范围
    [SerializeField] private LayerMask damageLayer; // 可伤害的层级

    [Header("引用")]
    [SerializeField] private Transform targetTransform; // 目标Transform（通常是Boss）
    
    /// <summary>
    /// 设置圣枪的伤害值
    /// </summary>
    /// <param name="damage">要设置的伤害值</param>
    public void SetDamageAmount(int damage)
    {
        damageAmount = damage;
    }
    
    /// <summary>
    /// 设置圣枪的目标
    /// </summary>
    /// <param name="target">目标Transform</param>
    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
    }

    private float currentSpeed; // 当前速度
    private bool isAttacking = false; // 是否正在攻击
    private Vector3 originalScale; // 原始缩放
    private GameObject trailEffect; // 当前的拖尾效果

    private void Start()
    {
        // 记录原始缩放
        originalScale = transform.localScale;
        
        // 初始设置为很小的尺寸，准备放大动画
        transform.localScale = Vector3.zero;

        // 如果没有设置精灵渲染器，尝试获取组件
        if (spearRenderer == null)
        {
            spearRenderer = GetComponent<SpriteRenderer>();
        }

        // 设置初始颜色
        if (spearRenderer != null)
        {
            spearRenderer.color = startColor;
        }

        // 如果没有设置目标，尝试查找Boss
        if (targetTransform == null)
        {
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
            {
                targetTransform = boss.transform;
            }
        }

        // 开始圣枪飞行协程
        StartCoroutine(HolySpearFlightCoroutine());
    }

    /// <summary>
    /// 圣枪飞行协程
    /// 控制整个飞行过程
    /// </summary>
    private IEnumerator HolySpearFlightCoroutine()
    {
        // 初始放大动画
        yield return StartCoroutine(ScaleUpCoroutine());

        // 初始化速度
        currentSpeed = initialSpeed;

        // 生成拖尾效果
        if (trailEffectPrefab != null)
        {
            trailEffect = Instantiate(trailEffectPrefab, transform);
        }

        // 播放飞行音效
        if (flySound != null)
        {
            AudioSource.PlayClipAtPoint(flySound, transform.position);
        }

        // 等待攻击前的延迟
        yield return new WaitForSeconds(delayBeforeAttack);

        // 设置为正在攻击状态
        isAttacking = true;

        // 飞行计时
        float flightTime = 0f;

        // 飞行循环
        while (flightTime < flightDuration)
        {
            // 更新飞行时间
            flightTime += Time.deltaTime;

            // 加速
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            // 如果有目标，朝向目标飞行
            if (targetTransform != null)
            {
                Vector3 direction = (targetTransform.position - transform.position).normalized;
                
                // 更新位置
                transform.position += direction * currentSpeed * Time.deltaTime;
                
                // 基本方向旋转 - 使圣枪朝向飞行方向
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                
                // 如果设置了始终旋转，则在朝向目标的基础上添加自旋
                if (alwaysSpin)
                {
                    // 先朝向目标
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    // 然后添加自旋
                    transform.Rotate(Vector3.forward, spinningSpeed * Time.deltaTime);
                }
                else
                {
                    // 只朝向目标旋转
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            // 如果没有目标，就直线飞行并旋转
            else
            {
                transform.position += transform.right * currentSpeed * Time.deltaTime;
                // 没有目标时始终旋转
                transform.Rotate(Vector3.forward, spinningSpeed * Time.deltaTime);
            }

            // 更新颜色渐变
            if (spearRenderer != null)
            {
                float colorProgress = Mathf.Clamp01(flightTime / flightDuration);
                spearRenderer.color = Color.Lerp(startColor, endColor, colorProgress);
            }

            // 检测碰撞
            if (isAttacking)
            {
                CheckCollision();
            }

            yield return null;
        }

        // 飞行时间结束，销毁圣枪
        DestroySpear();
    }

    /// <summary>
    /// 初始放大动画协程
    /// </summary>
    private IEnumerator ScaleUpCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < scaleUpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / scaleUpDuration);
            
            // 使用平滑的动画曲线
            float scaleProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            transform.localScale = originalScale * scaleProgress;
            yield return null;
        }

        // 确保最终缩放正确
        transform.localScale = originalScale;
    }

    /// <summary>
    /// 检查碰撞
    /// </summary>
    private void CheckCollision()
    {
        // 使用圆形检测查找碰撞（仅用于伤害检测）
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, damageLayer);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Boss"))
            {
                BossHealth bossHealth = hit.GetComponent<BossHealth>();
                if (bossHealth != null)
                {
                    bossHealth.TakeDamage(damageAmount);
                    Debug.Log("圣枪成功击中Boss，造成伤害: " + damageAmount);
                }
                
                // 移除这里的SetWeakState调用，改为在OnTriggerEnter2D中处理
                
                // 注意：伤害检测仍然保留，但虚弱状态设置移至物理碰撞处理
                // PlayHitEffect();
                // DestroySpear();
                break;
            }
        }
    }
    
    /// <summary>
    /// 当圣枪的Collider碰到其他Collider时触发
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boss"))
        {
            BossAI bossAI = other.GetComponent<BossAI>();
            if (bossAI != null)
            {
                bossAI.SetWeakState();
                Debug.Log("圣枪Collider碰到Boss并设置虚弱状态");
            }
            
            // 在物理碰撞时播放击中效果并销毁圣枪
            PlayHitEffect();
            DestroySpear();
        }
    }

    /// <summary>
    /// 播放击中效果
    /// </summary>
    private void PlayHitEffect()
    {
        // 生成击中效果
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // 2秒后销毁效果
        }

        // 播放击中音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        // 触发相机震动
        EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, new object[] { 0.5f, 0.7f });
    }

    /// <summary>
    /// 销毁圣枪
    /// </summary>
    private void DestroySpear()
    {
        // 销毁拖尾效果
        if (trailEffect != null)
        {
            Destroy(trailEffect);
        }

        // 销毁圣枪
        Destroy(gameObject);
    }

    /// <summary>
    /// 在编辑器中显示伤害范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, damageRadius);
    }
}