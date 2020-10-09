using System;
using System.Collections.Generic;

namespace MicroserviceFramework.EventBus
{
	public interface ISubscriptionInfoStore
	{
		void Add<TEvent, TEventHandler>()
			where TEvent : IntegrationEvent
			where TEventHandler : IIntegrationEventHandler<TEvent>;

		void Add(Type eventType, Type eventHandlerType);
		IReadOnlyCollection<SubscriptionInfo> GetHandlers(string eventType);

		string GetEventKey<T>();
		string GetEventKey(Type type);
		void Remove<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
	}
}