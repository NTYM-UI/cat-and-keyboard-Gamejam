# 项目设置文档

## 1. 项目概述

- **项目名称**：TapTap聚光灯21天GameJam
- **项目类型**：2D游戏
- **开发引擎**：Unity
- **团队成员**：策划1名，美术1名，程序3名（包括主程）

## 2. 项目目录结构

项目使用了清晰的目录结构来组织不同类型的资源和代码：

```
Assets/_Project/
├── 01_Scripts/         # 游戏代码
│   ├── Framework/      # 基础框架代码
│   ├── Gameplay/       # 核心玩法代码
│   │   ├── 01_Player/  # 玩家相关代码
│   │   ├── 02_Level/   # 关卡相关代码
│   │   └── 03_GameRule/# 游戏规则相关代码
│   └── UI/             # UI相关代码
├── 02_Resources/       # 资源文件
│   ├── 01_Sprites/     # 2D精灵图
│   ├── 02_Animations/  # 动画资源
│   ├── 03_Audio/       # 音频资源
│   └── 04_Fonts/       # 字体文件
├── 03_Scenes/          # 场景文件
├── 04_Prefabs/         # 预制体
├── 05_Art/             # 美术资源
└── 06_Others/          # 其他文件（插件、配置等）
```

## 3. 代码规范

### 3.1 命名规范

- **类名**：使用PascalCase（首字母大写，无下划线），例如：`PlayerController`
- **方法名**：使用PascalCase，例如：`MovePlayer()`
- **变量名**：使用camelCase（首字母小写，无下划线），例如：`playerHealth`
- **私有变量**：可以在前面加下划线，例如：`_playerSpeed`
- **常量**：使用全大写加下划线，例如：`MAX_HEALTH`
- **枚举**：使用PascalCase，枚举值使用全大写加下划线，例如：`enum GameState { GAME_READY, GAME_PLAYING }`
- **命名空间**：使用PascalCase，例如：`CatAndKeyboard.Gameplay`

### 3.2 注释规范

- 所有类都应该有类级别的注释，说明其功能和用途
- 关键方法应该有方法级别的注释，说明其参数、返回值和作用
- 复杂的逻辑应该有行级注释，解释代码的意图
- 使用`///`来创建XML文档注释，方便IDE显示提示信息

### 3.3 代码风格

- 使用4个空格进行缩进（而非Tab）
- 大括号`{`放在行尾，不单独占一行
- 每行代码不超过100个字符
- 避免过长的方法，尽量保持方法职责单一
- 使用空行分隔不同的逻辑块

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

## 5. 2D项目设置

### 5.1 项目设置

- **像素单位（Pixels Per Unit）**：100（可根据美术资源调整）
- **渲染管线**：Unity 2D Renderer
- **相机设置**：正交相机，Size根据游戏内容调整
- **物理设置**：使用2D物理系统（Physics 2D）

### 5.2 精灵设置

- **格式**：PNG
- **压缩**：根据平台选择合适的压缩格式
- **过滤模式**：Point（像素风格游戏）或Bilinear（平滑风格游戏）
- **精灵图集**：使用Sprite Atlas打包精灵，减少Draw Call

### 5.3 UI设置

- **画布渲染模式**：Screen Space - Camera
- **UI缩放模式**：Scale With Screen Size
- **参考分辨率**：根据目标设备设置，例如1920x1080

## 6. 团队协作流程

### 6.1 版本控制

- 使用Git进行版本控制
- 分支策略：Feature Branch Workflow
  - 主分支：main/master
  - 特性分支：feature/功能名称
- 提交信息规范：`[模块名] 功能简述`，例如：`[Player] 实现玩家跳跃功能`

### 6.2 工作流

1. 策划提供需求文档和设计规范
2. 美术根据设计规范创建资源
3. 程序实现功能并集成资源
4. 定期进行代码审查和测试
5. 每日进行项目进度同步

### 6.3 资源提交规范

- 美术资源需要按照指定的目录结构存放
- 资源命名需要遵循命名规范
- 提交前需要确认资源质量和格式符合要求

## 7. 性能优化建议

- 使用对象池管理频繁创建和销毁的对象
- 使用精灵图集减少Draw Call
- 避免在Update中进行复杂计算
- 使用异步加载处理大资源
- 定期使用Unity Profiler进行性能分析

## 8. 开发工具推荐

- **代码编辑器**：Visual Studio或Rider
- **美术工具**：Photoshop、Aseprite等
- **版本控制**：Git + GitHub/Gitee
- **协作工具**：Discord、TAPD等

## 9. 注意事项

- 保持代码简洁和可读性
- 遵循项目规范和流程
- 及时提交代码和更新进度
- 遇到问题及时沟通
- 确保代码能够正常运行和构建