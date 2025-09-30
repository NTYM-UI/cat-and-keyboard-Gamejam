using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using System;
using System.Globalization;

/// <summary>
/// 游戏工具类，提供常用的辅助方法
/// 使用示例：float distance = GameUtility.CalculateDistance(pos1, pos2);
/// </summary>
public static class GameUtility
{
    #region 数学计算相关

    /// <summary>
    /// 计算两点之间的欧几里得距离
    /// </summary>
    public static float CalculateDistance(Vector2 point1, Vector2 point2)
    {
        return Vector2.Distance(point1, point2);
    }

    /// <summary>
    /// 计算两点之间的曼哈顿距离
    /// </summary>
    public static float CalculateManhattanDistance(Vector2 point1, Vector2 point2)
    {
        return Mathf.Abs(point1.x - point2.x) + Mathf.Abs(point1.y - point2.y);
    }

    /// <summary>
    /// 计算两点之间的方向向量
    /// </summary>
    public static Vector2 CalculateDirection(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        if (direction.sqrMagnitude > 0)
        {
            direction.Normalize();
        }
        return direction;
    }

    /// <summary>
    /// 限制数值在指定范围内
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }

    /// <summary>
    /// 限制向量在指定长度内
    /// </summary>
    public static Vector2 ClampVector(Vector2 vector, float maxLength)
    {
        if (vector.sqrMagnitude > maxLength * maxLength)
        {
            vector = vector.normalized * maxLength;
        }
        return vector;
    }

    /// <summary>
    /// 角度转弧度
    /// </summary>
    public static float DegreeToRadian(float degree)
    {
        return degree * Mathf.Deg2Rad;
    }

    /// <summary>
    /// 弧度转角度
    /// </summary>
    public static float RadianToDegree(float radian)
    {
        return radian * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 计算两点之间的角度（相对于x轴正方向）
    /// </summary>
    public static float CalculateAngle(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    #endregion

    #region 坐标转换相关

    /// <summary>
    /// 将世界坐标转换为屏幕坐标
    /// </summary>
    public static Vector2 WorldToScreenPoint(Vector3 worldPoint, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        return camera.WorldToScreenPoint(worldPoint);
    }

    /// <summary>
    /// 将屏幕坐标转换为世界坐标
    /// </summary>
    public static Vector3 ScreenToWorldPoint(Vector2 screenPoint, float zDepth = 0f, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        Vector3 worldPoint = camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, zDepth));
        worldPoint.z = zDepth;
        return worldPoint;
    }

    /// <summary>
    /// 将UI坐标转换为世界坐标
    /// </summary>
    public static Vector3 UIToWorldPoint(RectTransform rectTransform, Camera uiCamera = null, Camera worldCamera = null)
    {
        if (uiCamera == null)
        {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                uiCamera = canvas.worldCamera;
            }
        }

        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, rectTransform.position);
        return ScreenToWorldPoint(screenPoint, 0f, worldCamera);
    }

    #endregion

    #region 随机数相关

    /// <summary>
    /// 获取指定范围内的随机整数
    /// </summary>
    public static int RandomRange(int min, int max)
    {
        return UnityEngine.Random.Range(min, max + 1);
    }

    /// <summary>
    /// 获取指定范围内的随机浮点数
    /// </summary>
    public static float RandomRange(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    /// 随机获取数组中的一个元素
    /// </summary>
    public static T GetRandomElement<T>(T[] array)
    {
        if (array == null || array.Length == 0)
        {
            return default(T);
        }
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    /// <summary>
    /// 随机获取列表中的一个元素
    /// </summary>
    public static T GetRandomElement<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return default(T);
        }
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>
    /// 按权重随机选择
    /// </summary>
    public static T WeightedRandom<T>(List<T> items, List<float> weights)
    {
        if (items == null || weights == null || items.Count == 0 || items.Count != weights.Count)
        {
            return default(T);
        }

        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            currentWeight += weights[i];
            if (randomValue <= currentWeight)
            {
                return items[i];
            }
        }

        return items[0];
    }

    #endregion

    #region 游戏对象相关

    /// <summary>
    /// 查找子对象（递归）
    /// </summary>
    public static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform foundChild = FindChildRecursive(child, childName);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取或添加组件
    /// </summary>
    public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    /// <summary>
    /// 激活/禁用游戏对象及其所有子对象
    /// </summary>
    public static void SetActiveRecursive(GameObject gameObject, bool active)
    {
        if (gameObject == null)
        {
            return;
        }

        gameObject.SetActive(active);

        foreach (Transform child in gameObject.transform)
        {
            SetActiveRecursive(child.gameObject, active);
        }
    }

    /// <summary>
    /// 销毁游戏对象（考虑编辑模式和运行模式）
    /// </summary>
    public static void SafeDestroy(GameObject gameObject, float delay = 0f)
    {
        if (gameObject == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
        else
#endif
        {
            UnityEngine.Object.Destroy(gameObject, delay);
        }
    }

    /// <summary>
    /// 销毁组件（考虑编辑模式和运行模式）
    /// </summary>
    public static void SafeDestroyComponent<T>(GameObject gameObject, float delay = 0f) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Object.DestroyImmediate(component);
        }
        else
#endif
        {
            UnityEngine.Object.Destroy(component, delay);
        }
    }

    #endregion

    #region UI相关

    /// <summary>
    /// 设置UI文本
    /// </summary>
    public static void SetUIText(Text textComponent, string content)
    {
        if (textComponent != null)
        {
            textComponent.text = content;
        }
    }

    /// <summary>
    /// 设置UI图像
    /// </summary>
    public static void SetUIImage(Image imageComponent, Sprite sprite)
    {
        if (imageComponent != null)
        {
            imageComponent.sprite = sprite;
        }
    }

    /// <summary>
    /// 设置UI滑块值
    /// </summary>
    public static void SetUISlider(Slider sliderComponent, float value)
    {
        if (sliderComponent != null)
        {
            sliderComponent.value = value;
        }
    }

    /// <summary>
    /// 设置UI按钮交互状态
    /// </summary>
    public static void SetUIButtonInteractable(Button buttonComponent, bool interactable)
    {
        if (buttonComponent != null)
        {
            buttonComponent.interactable = interactable;
        }
    }

    #endregion

    #region 字符串相关

    /// <summary>
    /// 格式化数字，添加千位分隔符
    /// </summary>
    public static string FormatNumber(long number)
    {
        return number.ToString("N0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 格式化浮点数，保留指定小数位数
    /// </summary>
    public static string FormatFloat(float value, int decimalPlaces = 2)
    {
        string format = "F" + decimalPlaces;
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 截断字符串，并在末尾添加省略号
    /// </summary>
    public static string TruncateString(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
        {
            return str;
        }
        return str.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// 生成唯一ID
    /// </summary>
    public static string GenerateUniqueID()
    {
        return Guid.NewGuid().ToString();
    }

    #endregion

    #region 时间相关

    /// <summary>
    /// 格式化时间（秒）为分:秒格式
    /// </summary>
    public static string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// 格式化时间（秒）为时:分:秒格式
    /// </summary>
    public static string FormatTimeLong(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        
        if (hours > 0)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
        else
        {
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    #endregion

    #region 集合相关

    /// <summary>
    /// 打乱列表顺序
    /// </summary>
    public static void ShuffleList<T>(List<T> list)
    {
        if (list == null || list.Count <= 1)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    /// <summary>
    /// 从列表中移除满足条件的所有元素
    /// </summary>
    public static void RemoveAll<T>(List<T> list, Func<T, bool> predicate)
    {
        if (list == null || predicate == null)
        {
            return;
        }

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 检查列表是否为空
    /// </summary>
    public static bool IsListEmpty<T>(List<T> list)
    {
        return list == null || list.Count == 0;
    }

    #endregion

    #region 调试相关

    /// <summary>
    /// 绘制调试线
    /// </summary>
    public static void DrawDebugLine(Vector3 start, Vector3 end, Color color = default, float duration = 0f)
    {
        if (color == default)
        {
            color = Color.white;
        }
        Debug.DrawLine(start, end, color, duration);
    }

    /// <summary>
    /// 绘制调试射线
    /// </summary>
    public static void DrawDebugRay(Vector3 origin, Vector3 direction, Color color = default, float duration = 0f)
    {
        if (color == default)
        {
            color = Color.white;
        }
        Debug.DrawRay(origin, direction, color, duration);
    }

    /// <summary>
    /// 绘制调试圆
    /// </summary>
    public static void DrawDebugCircle(Vector3 center, float radius, Color color = default, float duration = 0f)
    {
        if (color == default)
        {
            color = Color.white;
        }

        const int segments = 32;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;

            Vector3 point1 = center + Quaternion.Euler(0, 0, angle1) * (Vector3.right * radius);
            Vector3 point2 = center + Quaternion.Euler(0, 0, angle2) * (Vector3.right * radius);

            Debug.DrawLine(point1, point2, color, duration);
        }
    }

    #endregion
}