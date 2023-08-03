using System;
using System.Collections.Generic;

namespace MicroserviceFramework.EventBus;

/// <summary>
/// 相同的 EventName 可以对应不同的 EventType
/// 所以需要通过 EventName 找到多个 EventType, 找到 IEventHandler, 再通过 DI 创建出所有注册对象
/// </summary>
public static class EventHandlerDescriptorManager
{
    /// <summary>
    /// KEY: 事件名
    /// Value->Key: 事件处理器的类型
    /// Value->Value: 事件处理器的方法
    /// </summary>
    private static readonly
        Dictionary<string, Dictionary<Type, EventHandlerDescriptor>> Cache;

    private static readonly HashSet<string> EventTypes = new();

    static EventHandlerDescriptorManager()
    {
        Cache = new Dictionary<string, Dictionary<Type, EventHandlerDescriptor>>();
    }

    public static void Register(Type eventType, Type handlerType)
    {
        if (!eventType.IsEvent())
        {
            throw new ArgumentException($"{eventType} 不是事件类型");
        }

        var eventName = eventType.GetEventName();
        EventTypes.Add(eventName);

        Dictionary<Type, EventHandlerDescriptor> dict;
        if (!Cache.ContainsKey(eventName))
        {
            dict = new Dictionary<Type, EventHandlerDescriptor>();
            Cache.TryAdd(eventName, dict);
        }
        else
        {
            dict = Cache[eventName];
        }

        dict.TryAdd(handlerType, new EventHandlerDescriptor(eventType, handlerType));
    }

    public static ICollection<EventHandlerDescriptor> GetOrDefault(string eventName)
    {
        return Cache.TryGetValue(eventName, out var result) ? result.Values : null;
    }

    public static IReadOnlyCollection<string> GetEventTypes()
    {
        return EventTypes;
    }
}
