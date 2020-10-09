using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	public interface IEventDispatcher : IDisposable
	{
		bool Register<TEvent, TEventHandler>()
			where TEvent : DomainEvent
			where TEventHandler : IEventHandler<TEvent>;

		bool Register<TEvent>(Type handlerType)
			where TEvent : DomainEvent;

		bool Register(Type eventType, Type handlerType);

		Task DispatchAsync(DomainEvent @event);
	}
}