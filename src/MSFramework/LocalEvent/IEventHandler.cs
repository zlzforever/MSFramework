using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 事件处理器
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventHandler<in TEvent> : IDisposable
    where TEvent : EventBase
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
