using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

/// <summary>
/// 事件处理器
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventHandler<in TEvent> : IDisposable
    where TEvent : EventBase
{
    Task HandleAsync(TEvent @event);
}
