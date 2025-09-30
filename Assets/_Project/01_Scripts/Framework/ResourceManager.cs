using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 资源管理系统，用于加载和管理游戏中的资源
/// 使用示例：
/// ResourceManager.Instance.LoadResource<Sprite>("Player/PlayerIdle");
/// ResourceManager.Instance.LoadResourceAsync<GameObject>("Prefabs/Enemy", OnLoadComplete);
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    // 缓存已加载的资源
    private Dictionary<string, Object> _resourceCache = new Dictionary<string, Object>();
    // 记录正在加载的资源，避免重复加载
    private Dictionary<string, List<System.Action<Object>>> _loadingOperations = new Dictionary<string, List<System.Action<Object>>>();

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径（相对于Resources文件夹）</param>
    /// <returns>加载的资源</returns>
    public T LoadResource<T>(string path) where T : Object
    {
        // 检查缓存中是否已存在该资源
        string cacheKey = typeof(T).Name + ":" + path;
        if (_resourceCache.TryGetValue(cacheKey, out Object cachedResource))
        {
            return cachedResource as T;
        }

        // 从Resources文件夹加载资源
        T resource = Resources.Load<T>(path);
        if (resource != null)
        {
            // 将资源存入缓存
            _resourceCache[cacheKey] = resource;
        }
        else
        {
            Debug.LogError("[ResourceManager] 无法加载资源: " + path + " (类型: " + typeof(T).Name + ")");
        }

        return resource;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径（相对于Resources文件夹）</param>
    /// <param name="onComplete">加载完成回调</param>
    public void LoadResourceAsync<T>(string path, System.Action<T> onComplete) where T : Object
    {
        // 检查缓存中是否已存在该资源
        string cacheKey = typeof(T).Name + ":" + path;
        if (_resourceCache.TryGetValue(cacheKey, out Object cachedResource))
        {
            onComplete?.Invoke(cachedResource as T);
            return;
        }

        // 检查是否已有正在进行的加载操作
        if (_loadingOperations.TryGetValue(cacheKey, out List<System.Action<Object>> callbacks))
        {
            // 如果有，添加回调到列表，不重复加载
            callbacks.Add(obj => onComplete?.Invoke(obj as T));
            return;
        }

        // 创建新的加载操作
        callbacks = new List<System.Action<Object>>();
        if (onComplete != null)
        {
            callbacks.Add(obj => onComplete?.Invoke(obj as T));
        }
        _loadingOperations[cacheKey] = callbacks;

        // 启动异步加载协程
        StartCoroutine(LoadResourceAsyncCoroutine<T>(path, cacheKey));
    }

    /// <summary>
    /// 异步加载资源的协程
    /// </summary>
    private IEnumerator LoadResourceAsyncCoroutine<T>(string path, string cacheKey) where T : Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;

        T resource = request.asset as T;
        if (resource != null)
        {
            // 将资源存入缓存
            _resourceCache[cacheKey] = resource;
        }
        else
        {
            Debug.LogError("[ResourceManager] 异步加载资源失败: " + path + " (类型: " + typeof(T).Name + ")");
        }

        // 执行所有回调
        if (_loadingOperations.TryGetValue(cacheKey, out List<System.Action<Object>> callbacks))
        {
            foreach (var callback in callbacks)
            {
                callback?.Invoke(resource);
            }
            // 移除加载操作记录
            _loadingOperations.Remove(cacheKey);
        }
    }

    /// <summary>
    /// 实例化资源（支持从缓存加载）
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <returns>实例化的游戏对象</returns>
    public GameObject Instantiate(string path, Vector3 position = default, Quaternion rotation = default)
    {
        GameObject prefab = LoadResource<GameObject>(path);
        if (prefab != null)
        {
            return Object.Instantiate(prefab, position, rotation);
        }
        return null;
    }

    /// <summary>
    /// 异步实例化资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="onComplete">实例化完成回调</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    public void InstantiateAsync(string path, System.Action<GameObject> onComplete, Vector3 position = default, Quaternion rotation = default)
    {
        LoadResourceAsync<GameObject>(path, prefab =>
        {
            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab, position, rotation);
                onComplete?.Invoke(instance);
            }
            else
            {
                onComplete?.Invoke(null);
            }
        });
    }

    /// <summary>
    /// 从缓存中移除指定资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <typeparam name="T">资源类型</typeparam>
    public void UnloadResource<T>(string path) where T : Object
    {
        string cacheKey = typeof(T).Name + ":" + path;
        if (_resourceCache.ContainsKey(cacheKey))
        {
            _resourceCache.Remove(cacheKey);
            Debug.Log("[ResourceManager] 已从缓存中移除资源: " + path);
        }
    }

    /// <summary>
    /// 清除所有缓存的资源
    /// 注意：这不会释放资源，只会移除缓存引用
    /// </summary>
    public void ClearCache()
    {
        _resourceCache.Clear();
        Debug.Log("[ResourceManager] 已清除所有资源缓存");
    }

    /// <summary>
    /// 释放未使用的资源
    /// 通常在场景切换后调用
    /// </summary>
    public void UnloadUnusedResources()
    {
        Resources.UnloadUnusedAssets();
        Debug.Log("[ResourceManager] 已释放未使用的资源");
    }
}