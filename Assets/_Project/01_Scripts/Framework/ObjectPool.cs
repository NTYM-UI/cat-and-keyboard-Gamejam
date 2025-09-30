using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 对象池系统，用于管理游戏中频繁创建和销毁的对象
/// 使用示例：
/// ObjectPool.Instance.InitializePool("Bullet", bulletPrefab, 10);
/// GameObject bullet = ObjectPool.Instance.GetObject("Bullet");
/// ObjectPool.Instance.ReturnObject(bullet);
/// </summary>
public class ObjectPool : Singleton<ObjectPool>
{
    // 存储对象池的字典，键为对象池名称
    private Dictionary<string, PoolData> _pools = new Dictionary<string, PoolData>();
    // 所有对象池对象的父物体
    private Transform _poolRoot;

    /// <summary>
    /// 对象池数据类
    /// </summary>
    private class PoolData
    {
        public GameObject prefab; // 预制体
        public Queue<GameObject> inactiveObjects; // 非激活状态的对象队列
        public Transform parentTransform; // 父物体Transform
        public int maxSize; // 对象池最大容量
        public int activeCount = 0; // 当前激活的对象数量
    }

    protected override void Awake()
    {
        base.Awake();
        // 创建对象池根节点
        _poolRoot = new GameObject("ObjectPools").transform;
        _poolRoot.parent = transform;
        DontDestroyOnLoad(_poolRoot.gameObject);
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <param name="prefab">预制体</param>
    /// <param name="initialSize">初始大小</param>
    /// <param name="maxSize">最大大小（0表示无限制）</param>
    public void InitializePool(string poolName, GameObject prefab, int initialSize = 5, int maxSize = 0)
    {
        if (string.IsNullOrEmpty(poolName) || prefab == null)
        {
            Debug.LogError("[ObjectPool] 对象池名称或预制体不能为空");
            return;
        }

        if (_pools.ContainsKey(poolName))
        {
            Debug.LogWarning("[ObjectPool] 对象池" + poolName + "已经存在，将覆盖其配置");
        }

        // 创建对象池数据
        PoolData poolData = new PoolData();
        poolData.prefab = prefab;
        poolData.inactiveObjects = new Queue<GameObject>();
        poolData.maxSize = maxSize;

        // 创建该对象池的父物体
        GameObject poolParent = new GameObject(poolName + "_Pool");
        poolData.parentTransform = poolParent.transform;
        poolData.parentTransform.parent = _poolRoot;

        // 预先创建指定数量的对象
        for (int i = 0; i < initialSize; i++)
        {
            CreatePoolObject(poolName, poolData);
        }

        // 将对象池添加到字典中
        _pools[poolName] = poolData;

        Debug.Log("[ObjectPool] 已初始化对象池: " + poolName + ", 初始大小: " + initialSize + ", 最大大小: " + (maxSize > 0 ? maxSize.ToString() : "无限制"));
    }

    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <returns>对象实例</returns>
    public GameObject GetObject(string poolName)
    {
        if (!_pools.TryGetValue(poolName, out PoolData poolData))
        {
            Debug.LogError("[ObjectPool] 未找到对象池: " + poolName);
            return null;
        }

        GameObject obj = null;

        // 如果有非激活对象，则从队列中获取
        if (poolData.inactiveObjects.Count > 0)
        {
            obj = poolData.inactiveObjects.Dequeue();
        }
        else
        {
            // 如果没有非激活对象，则创建新的对象（如果未达到最大容量）
            if (poolData.maxSize <= 0 || poolData.activeCount < poolData.maxSize)
            {
                obj = CreatePoolObject(poolName, poolData);
            }
            else
            {
                Debug.LogWarning("[ObjectPool] 对象池" + poolName + "已达到最大容量: " + poolData.maxSize);
                return null;
            }
        }

        // 激活对象
        if (obj != null)
        {
            obj.SetActive(true);
            poolData.activeCount++;
        }

        return obj;
    }

    /// <summary>
    /// 从对象池获取对象并设置位置和旋转
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <returns>对象实例</returns>
    public GameObject GetObject(string poolName, Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetObject(poolName);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    /// <summary>
    /// 将对象返回对象池
    /// </summary>
    /// <param name="obj">要返回的对象</param>
    public void ReturnObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        // 检查对象是否属于某个对象池
        Transform current = obj.transform.parent;
        while (current != null && current != _poolRoot)
        {
            string poolName = current.name.Replace("_Pool", "");
            if (_pools.TryGetValue(poolName, out PoolData poolData))
            {
                // 非激活对象并放入队列
                obj.SetActive(false);
                poolData.inactiveObjects.Enqueue(obj);
                poolData.activeCount--;
                return;
            }
            current = current.parent;
        }

        // 如果对象不属于任何对象池，则直接销毁
        Debug.LogWarning("[ObjectPool] 返回的对象不属于任何对象池，将直接销毁: " + obj.name);
        Destroy(obj);
    }

    /// <summary>
    /// 创建池对象
    /// </summary>
    private GameObject CreatePoolObject(string poolName, PoolData poolData)
    {
        GameObject obj = Instantiate(poolData.prefab);
        obj.name = poolName + "_" + (poolData.activeCount + poolData.inactiveObjects.Count + 1);
        obj.transform.parent = poolData.parentTransform;
        obj.SetActive(false);
        poolData.inactiveObjects.Enqueue(obj);
        return obj;
    }

    /// <summary>
    /// 清空指定对象池
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    public void ClearPool(string poolName)
    {
        if (_pools.TryGetValue(poolName, out PoolData poolData))
        {
            // 销毁所有非激活对象
            while (poolData.inactiveObjects.Count > 0)
            {
                GameObject obj = poolData.inactiveObjects.Dequeue();
                Destroy(obj);
            }

            // 重置激活计数
            poolData.activeCount = 0;

            Debug.Log("[ObjectPool] 已清空对象池: " + poolName);
        }
    }

    /// <summary>
    /// 清空所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var poolName in _pools.Keys)
        {
            ClearPool(poolName);
        }
        Debug.Log("[ObjectPool] 已清空所有对象池");
    }

    /// <summary>
    /// 获取对象池信息
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <param name="activeCount">激活对象数量</param>
    /// <param name="inactiveCount">非激活对象数量</param>
    /// <param name="maxCount">最大对象数量</param>
    /// <returns>是否获取成功</returns>
    public bool GetPoolInfo(string poolName, out int activeCount, out int inactiveCount, out int maxCount)
    {
        activeCount = 0;
        inactiveCount = 0;
        maxCount = 0;

        if (_pools.TryGetValue(poolName, out PoolData poolData))
        {
            activeCount = poolData.activeCount;
            inactiveCount = poolData.inactiveObjects.Count;
            maxCount = poolData.maxSize;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查对象池是否存在
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <returns>是否存在</returns>
    public bool HasPool(string poolName)
    {
        return _pools.ContainsKey(poolName);
    }
}