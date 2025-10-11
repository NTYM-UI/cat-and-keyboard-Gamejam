## 2. 项目目录结构

项目使用了清晰的目录结构来组织不同类型的资源和代码：

```
Assets/_Project/
├── 01_Scripts/         # 游戏代码
│   ├── Framework/      # 基础框架代码
│   ├── Gameplay/       # 核心玩法代码
│   │   ├── Player/  # 玩家相关代码
│   │   ├── Level/   # 关卡相关代码
│   │   └── GameRule/# 游戏规则相关代码
│   └── UI/             # UI相关代码
├── 02_Resources/       # 资源文件
│   ├── Sprites/     # 2D精灵图
│   ├── Animations/  # 动画资源
│   ├── Audio/       # 音频资源
│   └── Fonts/       # 字体文件
├── 03_Scenes/          # 场景文件
├── 04_Prefabs/         # 预制体
├── 05_Art/             # 美术资源
└── 06_Others/          # 其他文件（插件、配置等）
```

## 4. 框架使用指南

### 4.1 单例模式（Singleton）

单例模式用于创建全局唯一的组件实例，方便在代码中访问。

**使用示例**：
```csharp
public class GameManager : Singleton<GameManager>
{
    // 你的代码
}

// 访问方式
GameManager.Instance.DoSomething();
```

### 4.2 事件管理（EventManager）

事件管理系统用于处理游戏中的事件分发和订阅，实现代码解耦。

**常用事件**：
- `SceneLoadStarted` - 场景加载开始时触发
- `SceneLoadProgress` - 场景加载进度更新时触发
- `SceneLoadCompleted` - 场景加载完成时触发

**使用示例**：
```csharp
// 订阅事件
void OnEnable()
{
    EventManager.Instance.Subscribe("PlayerDeath", OnPlayerDeath);
}

// 取消订阅事件
void OnDisable()
{
    EventManager.Instance.Unsubscribe("PlayerDeath", OnPlayerDeath);
}

// 事件处理函数
void OnPlayerDeath(object data)
{
    // 处理玩家死亡事件
}

// 发布事件
void PlayerDied()
{
    EventManager.Instance.Publish("PlayerDeath", playerData);
}
```

### 4.3 资源管理（ResourceManager）

资源管理系统用于加载和管理游戏中的资源。

**使用示例**：
```csharp
// 同步加载资源
Sprite playerSprite = ResourceManager.Instance.LoadResource<Sprite>("Player/PlayerIdle");

// 异步加载资源
ResourceManager.Instance.LoadResourceAsync<GameObject>("Prefabs/Enemy", (enemyPrefab) =>
{
    // 资源加载完成后的处理
});

// 实例化资源
GameObject enemyInstance = ResourceManager.Instance.Instantiate("Prefabs/Enemy", spawnPosition);
```

### 4.4 场景管理（SceneManager）

场景管理系统用于处理游戏中的场景加载和切换。

**使用示例**：
```csharp
// 同步加载场景
SceneManager.Instance.LoadScene("GameScene");

// 异步加载场景
SceneManager.Instance.LoadSceneAsync("GameScene", (sceneName) =>
{
    // 场景加载完成后的处理
});

// 获取当前场景名称
string currentScene = SceneManager.Instance.CurrentSceneName;
```

### 4.5 对象池（ObjectPool）

对象池系统用于管理游戏中频繁创建和销毁的对象，提高性能。

**使用示例**：
```csharp
// 初始化对象池
void Start()
{
    ObjectPool.Instance.InitializePool("Bullet", bulletPrefab, 10, 50);
}

// 从对象池获取对象
void FireBullet()
{
    GameObject bullet = ObjectPool.Instance.GetObject("Bullet", firePoint.position, Quaternion.identity);
    // 设置子弹属性和行为
}

// 将对象返回对象池
void OnBulletHit(Collider2D collision)
{
    // 处理碰撞逻辑
    ObjectPool.Instance.ReturnObject(gameObject);
}
```

### 4.6 工具类（GameUtility）

工具类提供了各种常用的辅助方法。

**常用方法**：
- `CalculateDistance` - 计算两点之间的距离
- `RandomRange` - 获取指定范围内的随机数
- `GetOrAddComponent` - 获取或添加组件
- `SafeDestroy` - 安全销毁游戏对象
- `FormatTime` - 格式化时间

**使用示例**：
```csharp
// 计算距离
float distance = GameUtility.CalculateDistance(playerPosition, enemyPosition);

// 获取或添加组件
Rigidbody2D rb = GameUtility.GetOrAddComponent<Rigidbody2D>(gameObject);

// 格式化时间
string timeString = GameUtility.FormatTime(remainingSeconds);
```