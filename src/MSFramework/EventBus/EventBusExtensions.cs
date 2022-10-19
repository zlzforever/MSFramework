using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace MicroserviceFramework.EventBus;

public static class EventBusExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> EventTypes;

    static EventBusExtensions() => EventTypes = new ConcurrentDictionary<Type, bool>();

    public static bool IsEvent(this Type eventType) =>
        eventType != null && EventTypes.GetOrAdd(eventType,
            type => type != null && typeof(EventBase).IsAssignableFrom(type));

    public static string GetEventName(this Type type)
    {
        var attribute = type.GetCustomAttribute<EventNameAttribute>();

        return attribute != null ? attribute.Name : type.FullName;
    }

    public static string GetEventName(this EventBase @event)
    {
        var type = @event.GetType();
        return GetEventName(type);
    }

    // public static MethodInfo GetHandlerMethod(this Type handlerType, Type eventType)
    // {
    // 	var type = typeof(IEventHandler<>).MakeGenericType(eventType);
    // 	return type.IsAssignableFrom(handlerType) ? type.GetMethod("HandleAsync", new[] { eventType }) : null;
    // }
}
