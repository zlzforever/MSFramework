using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
	public interface IEventBus
	{
		Task PublishAsync(Event @event);

		Task SubscribeAsync<T, TH>()
			where T : Event
			where TH : IEventHandler<T>;

		Task SubscribeAsync(Type eventType, Type handlerType);
		void Unsubscribe<T, TH>()
			where TH : IEventHandler<T>
			where T : Event;
	}
}