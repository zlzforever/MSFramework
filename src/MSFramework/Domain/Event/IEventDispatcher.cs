using System;
using System.Threading.Tasks;

namespace MSFramework.Domain.Event
{
	public interface IEventDispatcher
	{
		bool Register<TEvent, TEventHandler>()
			where TEvent : EventBase
			where TEventHandler : IEventHandler<TEvent>;

		bool Register<TEvent>(Type handlerType)
			where TEvent : EventBase;

		bool Register(Type eventType, Type handlerType);
		
		void Dispatch(IEvent @event);
	}
}