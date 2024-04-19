using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using MicroserviceFramework.Utils;

namespace MicroserviceFramework.LocalEvent;

public static class EventTypeExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> EventTypes;
    private static readonly ConcurrentDictionary<Type, (Type, Type, MethodInfo)> EventMetadata;

    static EventTypeExtensions()
    {
        EventTypes = new ConcurrentDictionary<Type, bool>();
        EventMetadata = new ConcurrentDictionary<Type, (Type, Type, MethodInfo)>();
    }

    private static bool IsEvent(this Type eventType)
    {
        return eventType != null && EventTypes.GetOrAdd(eventType,
            type => type != null && EventBase.EventBaseType.IsAssignableFrom(type));
    }

    public static (Type HandlerInterfaceType, Type ServiceType, MethodInfo Method) GetEventMetadata(this Type eventType)
    {
        Check.NotNull(eventType, nameof(eventType));

        if (!eventType.IsEvent())
        {
            throw new ArgumentException($"{eventType.Name} 不是事件类型");
        }

        return EventMetadata.GetOrAdd(eventType,
            t =>
            {
                var eventHandlerType = Defaults.EventHandlerType.MakeGenericType(t);
                var serviceType = typeof(IEnumerable<>).MakeGenericType(eventHandlerType);
                var handlerMethod = eventHandlerType.GetMethod("HandleAsync");
                if (handlerMethod == null)
                {
                    throw new ArgumentException($"{eventHandlerType.Name} 查询 HandleAsync 方法失败");
                }

                return (eventHandlerType, serviceType, handlerMethod);
            });
    }
}
