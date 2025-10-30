using UnityEngine;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    [Header("炸弹设置")]
    [SerializeField] private float lifetime = 3f;         // 炸弹存在时间
    [SerializeField] private float explosionRadius = 2f;   // 爆炸半径
    [SerializeField] private int damage = 1;              // 爆炸伤害
    [SerializeField] private GameObject explosionEffect;   // 爆炸特效
    
    [Header("动画设置")]
    [SerializeField] private float appearanceTime = 1f;   // 显示动画时间（从透明到不透明，从缩放0到1）
    [SerializeField] private float warningTime = 3f;    // 爆炸前预警时间
    [SerializeField] private float flashSpeed = 0.1f;     // 预警闪烁速度

    private Rigidbody2D rb;                                // 刚体组件
    private float spawnTime;                               // 生成时间
    private bool hasExploded = false;                      // 是否已爆炸
    private SpriteRenderer spriteRenderer;                 // 精灵渲染器，用于控制透明度
    private bool isWarningActive = false;                  // 是否正在预警中
    public bool isAppearing = true;                        // 是否正在显示动画（公开以便BossAI访问）

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 初始化spawnTime为一个非常大的值，表示爆炸计时器还未开始
        spawnTime = float.MaxValue;
        
        // 设置初始状态：缩放为0，透明度为0
        transform.localScale = Vector3.zero;
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
        
        // 如果有刚体组件，设置一些基本属性
        if (rb != null)
        {
            rb.gravityScale = 1f;  // 设置适当的重力
            // 弹跳效果现在通过物理材质处理
        }
        
        // 启动显示动画协程
        StartCoroutine(AppearanceAnimation());
    }
    
    // 显示动画协程 - 从透明到不透明，从缩放0到1
    private System.Collections.IEnumerator AppearanceAnimation()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < appearanceTime)
        {
            float progress = elapsedTime / appearanceTime;
            
            // 更新缩放
            transform.localScale = Vector3.one * progress;
            
            // 更新透明度
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = progress;
                spriteRenderer.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终状态正确
        transform.localScale = Vector3.one;
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        
        isAppearing = false;
    }
    
    // 开始爆炸计时器 - 从炸弹被投掷出去时开始计算爆炸时间
    public void StartExplosionTimer()
    {
        spawnTime = Time.time;  // 设置为当前时间，开始计算爆炸时间
    }

    private void Update()
    {
        // 移除旋转逻辑
        
        // 检查爆炸计时器是否已开始，并且是否超过生命周期
        // spawnTime为float.MaxValue表示计时器还未开始
        if (spawnTime < float.MaxValue && Time.time >= spawnTime + lifetime && !hasExploded)
        {
            Explode(true); // 传递true表示有特效和伤害
            EventManager.Instance.Publish(GameEventNames.PLAY_BOMB_SOUND);
        }
        
        // 检查是否需要开始预警
        if (spawnTime < float.MaxValue && !isWarningActive && !hasExploded)
        {
            float timeUntilExplosion = spawnTime + lifetime - Time.time;
            if (timeUntilExplosion <= warningTime)
            {
                isWarningActive = true;
                StartCoroutine(WarningAnimation());
            }
        }
    }
    
    // 预警动画协程 - 炸弹即将爆炸时的闪烁效果（速度随时间加快）
    private System.Collections.IEnumerator WarningAnimation()
    {
        if (spriteRenderer == null) yield break;
        
        bool flashState = false;
        float originalAlpha = spriteRenderer.color.a;
        float minFlashSpeed = flashSpeed * 0.2f; // 最小闪烁速度，避免闪烁过快
        
        // 持续闪烁直到爆炸
        while (!hasExploded)
        {
            // 切换闪烁状态
            flashState = !flashState;
            
            // 设置颜色 - 闪烁时变红
            Color color = spriteRenderer.color;
            if (flashState)
            {
                color = new Color(1f, 0.2f, 0.2f, originalAlpha); // 红色闪烁
            }
            else
            {
                color = new Color(1f, 1f, 1f, originalAlpha); // 恢复原始颜色
            }
            spriteRenderer.color = color;
            
            // 计算当前剩余的爆炸时间
            float timeUntilExplosion = spawnTime + lifetime - Time.time;
            
            // 根据剩余时间动态调整闪烁速度
            // 剩余时间越少，闪烁速度越快
            float currentFlashSpeed;
            if (timeUntilExplosion <= 0)
            {
                currentFlashSpeed = minFlashSpeed;
            }
            else
            {
                // 计算闪烁速度比例：剩余时间/预警总时间
                // 当剩余时间是预警时间的一半时，闪烁速度是基础速度的1.5倍
                // 当剩余时间接近0时，闪烁速度接近最小限制
                float speedRatio = timeUntilExplosion / warningTime;
                currentFlashSpeed = Mathf.Lerp(minFlashSpeed, flashSpeed, speedRatio);
            }
            
            yield return new WaitForSeconds(currentFlashSpeed);
        }
    }

    // 爆炸方法 - 重载版本，允许指定是否有特效和伤害
    private void Explode(bool hasEffectAndDamage = false)
    {
        if (hasExploded) return;
        
        hasExploded = true;
        
        // 只有在指定有特效和伤害时才创建特效
        if (hasEffectAndDamage && explosionEffect != null)
        {
            // 触发相机震动效果
            CameraShakeHelper.TriggerCameraShake();
            GameObject effectInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            // 设置爆炸特效在2秒后自动销毁，避免特效永久存在
            Destroy(effectInstance, 2f);
        }
        
        // 只有在指定有特效和伤害时才造成伤害
        if (hasEffectAndDamage)
        {
            // 检测爆炸范围内的所有碰撞器
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            
            // 对每个碰撞器应用伤害 - 只对Player标签的对象造成伤害
            foreach (Collider2D collider in hitColliders)
            {
                // 只对Player标签的对象造成伤害
                if (collider.CompareTag("Player"))
                {
                    // 尝试获取PlayerHealth组件并造成伤害
                    PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }
            }
        }
        
        // 销毁炸弹对象
        Destroy(gameObject);
    }
    
    // 绘制爆炸范围的 gizmo，在游戏运行时也可见
    private void OnDrawGizmos()
    {
        // 确保只有在游戏运行时才显示，避免在编辑器中一直显示
        if (Application.isPlaying)
        {
            // 使用半透明红色表示爆炸范围
            Color transparentRed = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.color = transparentRed;
            // 绘制半透明实心球体表示爆炸范围
            Gizmos.DrawSphere(transform.position, explosionRadius);
            
            // 添加一个红色线框，增强可见性
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
    
    // 绘制爆炸范围的 gizmo，方便编辑时调试（仅在选择对象时显示）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    
    // 当炸弹与其他碰撞体发生碰撞时触发
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否与玩家发生碰撞
        if (collision.gameObject.CompareTag("Player") && !hasExploded && !isAppearing)
        {
            // 直接爆炸并造成伤害和特效
            Explode(true);
            EventManager.Instance.Publish(GameEventNames.PLAY_BOMB_SOUND);
        }
    }
}