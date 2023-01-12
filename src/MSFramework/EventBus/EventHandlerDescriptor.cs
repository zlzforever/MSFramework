using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus;

public class EventHandlerDescriptor
{
    /// <summary>
    /// 事件的类型信息
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// 事件处理器的类型信息
    /// </summary>
    public Type HandlerType { get; }

    /// <summary>
    /// 事件处理器处理事件的方法
    /// </summary>
    public MethodInfo HandlerMethodInfo { get; }

    public EventHandlerDescriptor(Type eventType, Type eventHandlerType)
    {
        Type = eventType;
        HandlerType = eventHandlerType;
        HandlerMethodInfo = eventHandlerType.GetMethod("HandleAsync", new[] { eventType });
    }
}
