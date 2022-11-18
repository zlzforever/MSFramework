using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus;

public class SubscriptionInfo
{
    /// <summary>
    /// 事件的类型信息
    /// </summary>
    public Type EventType { get; }

    /// <summary>
    /// 事件处理器的类型信息
    /// </summary>
    public Type EventHandlerType { get; }

    /// <summary>
    /// 事件处理器处理事件的方法
    /// </summary>
    public MethodInfo MethodInfo { get; }

    public SubscriptionInfo(Type eventType, Type eventHandlerType)
    {
        EventType = eventType;
        EventHandlerType = eventHandlerType;
        MethodInfo = eventHandlerType.GetMethod("HandleAsync", new[] { eventType });
    }
}
