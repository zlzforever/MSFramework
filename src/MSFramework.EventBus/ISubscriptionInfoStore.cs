using System;
using System.Collections.Generic;

namespace MicroserviceFramework.EventBus
{
	public interface ISubscriptionInfoStore
	{
		void Add<TEvent, TEventHandler>()
			where TEvent : Event
			where TEventHandler : IEventHandler<TEvent>;

		void Add(Type eventType, Type eventHandlerType);
		IReadOnlyCollection<SubscriptionInfo> GetHandlers(string eventType);

		string GetEventKey<T>();
		string GetEventKey(Type type);
		void Remove<TEvent, TEventHandler>() where TEvent : Event where TEventHandler : IEventHandler<TEvent>;
	}
}