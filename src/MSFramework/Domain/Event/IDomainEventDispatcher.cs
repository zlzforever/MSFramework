using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	public interface IDomainEventDispatcher : IDisposable
	{
		bool Register<TEvent, TEventHandler>()
			where TEvent : DomainEvent
			where TEventHandler : IDomainEventHandler<TEvent>;

		bool Register<TEvent>(Type handlerType)
			where TEvent : DomainEvent;

		bool Register(Type eventType, Type handlerType);

		Task DispatchAsync(DomainEvent @event);
	}
}