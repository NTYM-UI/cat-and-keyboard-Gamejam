using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 事件管理系统，用于处理游戏中的事件分发和订阅
/// 使用示例：
/// EventManager.Instance.Subscribe("PlayerDeath", OnPlayerDeath);
/// EventManager.Instance.Publish("PlayerDeath", playerData);
/// </summary>
public class EventManager : Singleton<EventManager>
{
    // 存储事件及其对应的回调函数列表
    private Dictionary<string, Action<object>> _eventDictionary = new Dictionary<string, Action<object>>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    public void Subscribe(string eventName, Action<object> callback)
    {
        if (_eventDictionary.TryGetValue(eventName, out Action<object> eventActions))
        {
            // 如果事件已存在，则添加新的回调
            eventActions += callback;
            _eventDictionary[eventName] = eventActions;
        }
        else
        {
            // 如果事件不存在，则创建新的事件
            eventActions = callback;
            _eventDictionary.Add(eventName, eventActions);
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    public void Unsubscribe(string eventName, Action<object> callback)
    {
        if (_eventDictionary.TryGetValue(eventName, out Action<object> eventActions))
        {
            // 移除指定的回调
            eventActions -= callback;
            
            // 如果没有回调了，则移除该事件
            if (eventActions == null || eventActions.GetInvocationList().Length == 0)
            {
                _eventDictionary.Remove(eventName);
            }
            else
            {
                _eventDictionary[eventName] = eventActions;
            }
        }
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="eventData">事件数据</param>
    public void Publish(string eventName, object eventData = null)
    {
        if (_eventDictionary.TryGetValue(eventName, out Action<object> eventActions))
        {
            // 执行所有订阅该事件的回调
            try
            {
                eventActions.Invoke(eventData);
            }
            catch (Exception e)
            {
                Debug.LogError("[EventManager] 发布事件时出错: " + eventName + "\n" + e.ToString());
            }
        }
        else
        {
            // 可选：发布未订阅的事件时给出警告
            // Debug.LogWarning("[EventManager] 发布了未被订阅的事件: " + eventName);
        }
    }

    /// <summary>
    /// 检查事件是否有订阅者
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>是否有订阅者</returns>
    public bool HasSubscribers(string eventName)
    {
        return _eventDictionary.ContainsKey(eventName);
    }

    /// <summary>
    /// 清除所有事件
    /// 通常在场景切换时使用
    /// </summary>
    public void ClearAllEvents()
    {
        _eventDictionary.Clear();
        Debug.Log("[EventManager] 已清除所有事件");
    }

    /// <summary>
    /// 清除特定事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    public void ClearEvent(string eventName)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            _eventDictionary.Remove(eventName);
            Debug.Log("[EventManager] 已清除事件: " + eventName);
        }
    }
}