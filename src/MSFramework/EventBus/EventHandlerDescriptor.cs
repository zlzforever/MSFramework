using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus;

public class EventHandlerDescriptor
{
    /// <summary>
    /// 事件的类型信息
    /// </summary>
    public Type EventType { get; }

    /// <summary>
    /// 事件处理器的类型信息
    /// </summary>
    public Type HandlerType { get; }

    /// <summary>
    /// 事件处理器处理事件的方法
    /// </summary>
    public MethodInfo HandleMethod { get; }

    public EventHandlerDescriptor(Type eventType, Type eventHandlerType)
    {
        EventType = eventType;
        HandlerType = eventHandlerType;
        HandleMethod = eventHandlerType.GetMethod("HandleAsync", new[] { eventType });
    }
}
