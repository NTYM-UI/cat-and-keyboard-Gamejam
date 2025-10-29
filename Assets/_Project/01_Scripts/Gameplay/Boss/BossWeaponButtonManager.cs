using UnityEngine;
using System.Collections;

/// <summary>
/// 管理Boss战中的武器按钮生成和逻辑
/// 当玩家坚持一段时间后生成按钮，玩家踩下后触发圣枪攻击
/// </summary>
public class BossWeaponButtonManager : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private GameObject weaponButtonPrefab; // 按钮预制体
    [SerializeField] private float timeToSpawnButton = 30f; // 生成按钮所需的坚持时间（秒）
    [SerializeField] private Vector3 buttonSpawnPosition = new Vector3(0f, 0f, 0f); // 按钮生成位置
    [SerializeField] private float buttonSpawnRandomRange = 5f; // 按钮生成位置的随机范围
    [SerializeField] private float buttonLifetime = 15f; // 按钮存在的时间
    [SerializeField] private float buttonCooldown = 45f; // 按钮消失后再次生成的冷却时间
    
    [Header("地图边界设置")]
    [SerializeField] private bool useMapBounds = true; // 是否使用地图边界限制
    [SerializeField] private float mapMinX = -10f; // 地图最小X坐标
    [SerializeField] private float mapMaxX = 10f; // 地图最大X坐标
    [SerializeField] private float groundOffset = 0f; // 地面偏移量，确保按钮生成在地面上

    [Header("圣枪设置")]
    [SerializeField] private GameObject holySpearPrefab; // 圣枪预制体
    [SerializeField] private float spearDamage = 90f; // 圣枪造成的伤害
    [SerializeField] private float cameraShakeDuration = 0.5f; // 相机震动持续时间
    [SerializeField] private float cameraShakeMagnitude = 0.5f; // 相机震动幅度

    [Header("引用")]
    [SerializeField] private Transform bossTransform; // Boss的Transform
    [SerializeField] private Transform playerTransform; // 玩家的Transform

    private bool isButtonSpawned = false; // 按钮是否已生成
    private GameObject currentButton = null; // 当前生成的按钮
    private float battleStartTime; // 战斗开始时间
    private float lastButtonSpawnTime; // 上次按钮生成时间
    private bool battleStarted = false; // 战斗是否已开始

    private void Start()
    {
        // 初始设置最后生成时间为负冷却时间，允许第一次生成
        lastButtonSpawnTime = -buttonCooldown;

        // 订阅按钮按下事件
        EventManager.Instance.Subscribe(GameEventNames.BOSS_WEAPON_BUTTON_PRESSED, OnButtonPressed);
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe(GameEventNames.BOSS_WEAPON_BUTTON_PRESSED, OnButtonPressed);
    }

    private void Update()
    {
        // 检查Boss是否显示并且战斗尚未开始
        if (!battleStarted && bossTransform != null && bossTransform.gameObject.activeInHierarchy)
        {
            // Boss显示，战斗开始
            battleStartTime = Time.time;
            battleStarted = true;
            Debug.Log("Boss显示，战斗开始，按钮生成计时器启动");
        }
        
        // 只有在战斗开始后才检查生成按钮
        if (!battleStarted) return;
        
        // 检查是否应该生成按钮
        if (!isButtonSpawned && 
            Time.time >= battleStartTime + timeToSpawnButton && 
            Time.time >= lastButtonSpawnTime + buttonCooldown)
        {
            SpawnWeaponButton();
        }
    }

    /// <summary>
    /// 生成武器按钮
    /// </summary>
    private void SpawnWeaponButton()
    {
        if (weaponButtonPrefab == null) return;

        // 计算按钮生成位置
        Vector3 spawnPosition = CalculateButtonSpawnPosition();

        // 生成按钮
        currentButton = Instantiate(weaponButtonPrefab, spawnPosition, Quaternion.identity);
        isButtonSpawned = true;
        lastButtonSpawnTime = Time.time;

        // 设置按钮的寿命
        Destroy(currentButton, buttonLifetime);

        // 发布按钮生成事件
        EventManager.Instance.Publish(GameEventNames.BOSS_WEAPON_BUTTON_SPAWN, spawnPosition);
    }

    /// <summary>
    /// 计算按钮生成位置
    /// </summary>
    /// <returns>按钮的生成位置</returns>
    private Vector3 CalculateButtonSpawnPosition()
    {
        Vector3 position;
        int attempts = 0;
        
        // 确定基准位置
        Vector3 basePosition;
        if (buttonSpawnPosition != Vector3.zero)
        {
            basePosition = buttonSpawnPosition;
        }
        else if (playerTransform != null)
        {
            basePosition = playerTransform.position;
        }
        else
        {
            // 如果都没有，则使用默认位置
            basePosition = new Vector3(0f, groundOffset, 0f);
        }
        
        do
        {
            attempts++;
            
            // 生成随机偏移
            float randomX = Random.Range(-buttonSpawnRandomRange, buttonSpawnRandomRange);
            float randomY = 0f; // 先设置为0，之后会根据是否使用边界调整
            
            // 计算初始位置
            position = new Vector3(
                basePosition.x + randomX,
                basePosition.y + randomY,
                0f
            );
            
            // 应用地图边界限制
            if (useMapBounds)
            {
                // 限制X坐标在地图范围内
                position.x = Mathf.Clamp(position.x, mapMinX, mapMaxX);
                
                // 将Y坐标设置为地面偏移量
                position.y = groundOffset;
            }
            
            // 如果尝试次数过多，就不再检查Boss位置
            if (attempts > 20) break;
            
            // 检查是否太靠近Boss
        } while (bossTransform != null && 
                Vector3.Distance(position, bossTransform.position) < 3f);
        
        // 确保最终位置在有效范围内
        if (useMapBounds)
        {
            position.x = Mathf.Clamp(position.x, mapMinX, mapMaxX);
            position.y = groundOffset;
        }
        
        return position;
    }

    /// <summary>
    /// 处理按钮被按下的事件
    /// </summary>
    private void OnButtonPressed(object data)
    {
        // 清除当前按钮状态
        isButtonSpawned = false;
        if (currentButton != null)
        {
            Destroy(currentButton);
            currentButton = null;
        }

        // 执行圣枪攻击
        ExecuteHolySpearAttack();
    }

    /// <summary>
    /// 执行圣枪攻击
    /// </summary>
    private void ExecuteHolySpearAttack()
    {
        // 实例化圣枪预制体
        if (holySpearPrefab != null)
        {
            // 确定圣枪生成位置（从屏幕外左侧或右侧）
            Vector3 spawnPosition = CalculateSpearSpawnPosition();
            
            // 实例化圣枪
            GameObject spearInstance = Instantiate(holySpearPrefab, spawnPosition, Quaternion.identity);
            
            // 获取圣枪控制器组件
            HolySpearController spearController = spearInstance.GetComponent<HolySpearController>();
            if (spearController != null)
            {
                // 使用公共方法设置伤害值
                int damageAmount = Mathf.RoundToInt(spearDamage);
                spearController.SetDamageAmount(damageAmount);
                
                // 如果有Boss引用，使用公共方法设置目标
                if (bossTransform != null)
                {
                    spearController.SetTargetTransform(bossTransform);
                    Debug.Log($"圣枪已设置目标为Boss，伤害值: {damageAmount}");
                }
                else
                {
                    Debug.LogWarning("未能找到Boss或Boss transform，圣枪将使用旋转功能");
                }
            }
            
            Debug.Log("圣枪已生成，位置: " + spawnPosition);
        }
        else
        {
            Debug.LogWarning("holySpearPrefab 未设置，请在Inspector中分配圣枪预制体!");
        }

        // 发布圣枪攻击事件
        EventManager.Instance.Publish(GameEventNames.HOLY_SPEAR_ATTACK, spearDamage);

        // 移除直接调用BossAI方法的代码，因为现在由圣枪自身的碰撞检测来处理伤害和虚弱状态
        // 这样可以确保无论Boss处于什么状态（包括释放技能时），只要被圣枪击中就会受到伤害

        // 触发相机震动
        EventManager.Instance.Publish(GameEventNames.CAMERA_SHAKE, new object[] { cameraShakeDuration, cameraShakeMagnitude });

        Debug.Log("圣枪攻击执行，伤害值设置为: " + spearDamage);
    }
    
    // 移除了OnBossBattleStart方法，改为在Update中检测Boss的显示状态
    
    /// <summary>
    /// 计算圣枪生成位置
    /// </summary>
    /// <returns>圣枪的生成位置（屏幕外）</returns>
    private Vector3 CalculateSpearSpawnPosition()
    {
        // 获取屏幕边界
        float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        
        // 随机从屏幕左侧或右侧生成
        bool spawnOnLeft = Random.value > 0.5f;
        
        // 计算X坐标（屏幕外10%的位置）
        float spawnX = spawnOnLeft ? -screenHalfWidth * 1.1f : screenHalfWidth * 1.1f;
        
        // 计算Y坐标（屏幕中心偏上）
        float spawnY = Camera.main.orthographicSize * 0.5f;
        
        return new Vector3(spawnX, spawnY, 0f);
    }
}