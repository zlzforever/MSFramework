using System;
using System.Collections.Generic;
using System.Threading;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
///
/// </summary>
public class EventDescriptorStore
{
    private readonly Dictionary<Type, HashSet<EventDescriptor>> _descriptors =
        new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="handlerType"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Register(Type eventType, Type handlerType)
    {
        var method = handlerType.GetMethod("HandleAsync", [eventType, typeof(CancellationToken)]);
        if (method == null)
        {
            throw new ArgumentException($"事件处理器 {handlerType.FullName} 没有实现事件 {eventType.FullName} 的处理方法");
        }

        if (!_descriptors.ContainsKey(eventType))
        {
            _descriptors.Add(eventType, new HashSet<EventDescriptor>());
        }

        _descriptors[eventType].Add(new EventDescriptor(handlerType, method));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns></returns>
    public IReadOnlyCollection<EventDescriptor> GetList(Type eventType)
    {
        return _descriptors.TryGetValue(eventType, out var v) ? v : Array.Empty<EventDescriptor>();
    }
}
