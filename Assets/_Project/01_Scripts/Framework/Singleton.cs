using UnityEngine;

/// <summary>
/// 单例模式基类，用于创建全局唯一的组件实例
/// 使用示例：public class GameManager : Singleton<GameManager> { ... }
/// </summary>
/// <typeparam name="T">继承该类的具体类型</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// 获取单例实例，如果不存在则创建一个
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] 应用程序正在退出，返回null：" + typeof(T));
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError("[Singleton] 在场景中发现多个" + typeof(T).Name + "实例！");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject(typeof(T).Name + "(Singleton)");
                        _instance = singleton.AddComponent<T>();
                        DontDestroyOnLoad(singleton);
                        Debug.Log("[Singleton] 创建了新的" + typeof(T).Name + "实例：" + singleton);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// 检查单例实例是否存在
    /// </summary>
    public static bool HasInstance
    {
        get { return _instance != null; }
    }

    /// <summary>
    /// 应用程序退出时调用，防止Unity编辑器模式下的空引用
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    /// <summary>
    /// 虚方法，允许子类重写Awake方法
    /// </summary>
    protected virtual void Awake()
    {
        // 可以在子类中重写此方法
    }

    /// <summary>
    /// 销毁时重置标记
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            _applicationIsQuitting = false;
        }
    }
}