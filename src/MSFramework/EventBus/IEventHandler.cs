using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
    public interface IEventHandler<in TEvent> : IDisposable
        where TEvent : EventBase
    {
        Task HandleAsync(TEvent @event);
    }
}