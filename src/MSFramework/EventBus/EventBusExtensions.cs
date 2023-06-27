using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MicroserviceFramework.EventBus;

public static class EventBusExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> EventTypes;

    static EventBusExtensions() => EventTypes = new ConcurrentDictionary<Type, bool>();

    public static bool IsEvent(this Type eventType)
    {
        return eventType != null && EventTypes.GetOrAdd(eventType,
            type => type != null && EventBase.Type.IsAssignableFrom(type));
    }

    public static string GetEventName(this Type type)
    {
        if (!type.IsEvent())
        {
            throw new MicroserviceFrameworkException($"类型 {type.FullName} 不是事件");
        }

        var attribute = type.GetCustomAttribute<EventNameAttribute>();
        return attribute != null ? attribute.Name : type.FullName;
    }

    // public static MethodInfo GetHandlerMethod(this Type handlerType, Type eventType)
    // {
    // 	var type = typeof(IEventHandler<>).MakeGenericType(eventType);
    // 	return type.IsAssignableFrom(handlerType) ? type.GetMethod("HandleAsync", new[] { eventType }) : null;
    // }
}
