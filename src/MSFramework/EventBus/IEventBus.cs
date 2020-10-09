using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
	public interface IEventBus
	{
		Task PublishAsync(IntegrationEvent @event);

		Task SubscribeAsync<T, TH>()
			where T : IntegrationEvent
			where TH : IIntegrationEventHandler<T>;

		Task SubscribeAsync(Type eventType, Type handlerType);
		void Unsubscribe<T, TH>()
			where TH : IIntegrationEventHandler<T>
			where T : IntegrationEvent;
	}
}