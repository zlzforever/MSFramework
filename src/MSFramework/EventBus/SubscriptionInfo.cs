using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus;

public class SubscriptionInfo
{
    public Type EventType { get; }
    public Type EventHandlerType { get; }
    public MethodInfo MethodInfo { get; }

    public SubscriptionInfo(Type eventType, Type eventHandlerType)
    {
        EventType = eventType;
        EventHandlerType = eventHandlerType;
        MethodInfo = eventHandlerType.GetMethod("HandleAsync", new[] { eventType });
    }
}
