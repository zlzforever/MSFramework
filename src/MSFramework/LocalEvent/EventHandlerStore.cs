using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace MicroserviceFramework.LocalEvent;

public record EventDescriptor(Type HandlerType, MethodInfo HandleMethod);

public static class EventHandlerStore
{
    private static readonly Dictionary<Type, HashSet<EventDescriptor>> Descriptors =
        new();

    public static IReadOnlyCollection<EventDescriptor> Get(Type eventType)
    {
        if (!Descriptors.TryGetValue(eventType, out var value))
        {
            return Array.Empty<EventDescriptor>();
        }

        return value;
    }

    public static void Add(Type eventType, Type handlerType)
    {
        var method = handlerType.GetMethod("HandleAsync", [eventType, typeof(CancellationToken)]);
        if (method == null)
        {
            throw new ArgumentException($"事件处理器 {handlerType.FullName} 没有实现事件 {eventType.FullName} 的处理方法");
        }

        if (!Descriptors.ContainsKey(eventType))
        {
            Descriptors.Add(eventType, new HashSet<EventDescriptor>());
        }

        Descriptors[eventType].Add(new EventDescriptor(handlerType, method));
    }
}
