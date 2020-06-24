using System;
using System.Threading.Tasks;

namespace MSFramework.Domain.Event
{
	public interface IEventMediator
	{
		bool Register<TEvent, TEventHandler>()
			where TEvent : EventBase
			where TEventHandler : IEventHandler<TEvent>;

		bool Register<TEvent>(Type handlerType)
			where TEvent : EventBase;

		bool Register(Type eventType, Type handlerType);
		
		Task PublishAsync(IEvent @event);
	}
}