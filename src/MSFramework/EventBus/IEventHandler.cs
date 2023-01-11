using System;
using System.Threading.Tasks;
using MicroserviceFramework.Application;

namespace MicroserviceFramework.EventBus;

public interface IEventHandler<in TEvent> : IDisposable
    where TEvent : EventBase
{
    Task HandleAsync(TEvent @event);
}
