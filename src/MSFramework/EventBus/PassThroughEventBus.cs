using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MSFramework.EventBus
{
	public class PassThroughEventBus : IPassThroughEventBus
	{
		private readonly IEventBusSubscriptionStore _store;
		private readonly ILogger _logger;
		private readonly IServiceProvider _serviceProvider;

		public PassThroughEventBus(ILogger<PassThroughEventBus> logger, IServiceProvider serviceProvider)
		{
			_store = new InMemoryEventBusSubscriptionStore();
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		public async Task PublishAsync(IEvent @event)
		{
			var eventName = _store.GetEventKey(@event.GetType());
			if (_store.ContainSubscription(eventName))
			{
				var subscriptions = _store.GetHandlers(eventName);
				foreach (var subscription in subscriptions)
				{
					if (subscription.IsDynamic)
					{
						var handler =
							_serviceProvider.GetService(subscription.HandlerType) as IDynamicEventHandler;
						if (handler == null) continue;
						await handler.Handle(@event);
					}
					else
					{
						var handler = _serviceProvider.GetService(subscription.HandlerType);
						if (handler == null) continue;

						var concreteType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
						// ReSharper disable once PossibleNullReferenceException
						await (Task) concreteType.GetMethod("Handle").Invoke(handler, new object[] {@event});
					}
				}
			}
		}
		
		public void Subscribe<TEvent, TEventHandler>() where TEvent : class, IEvent where TEventHandler : IEventHandler<TEvent>
		{
			var eventName = _store.GetEventKey<TEvent>();

			_logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName,
				typeof(TEventHandler).GetEventName());

			_store.AddSubscription<TEvent, TEventHandler>();
		}

		public void Subscribe<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			_logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName,
				typeof(TEventHandler).GetEventName());
			_store.AddSubscription<TEventHandler>(eventName);
		}

		public void Unsubscribe<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			_store.RemoveSubscription<TEventHandler>(eventName);
		}

		public void Unsubscribe<TEvent, TEventHandler>() where TEvent : class, IEvent where TEventHandler : IEventHandler<TEvent>
		{
			var eventName = _store.GetEventKey<TEvent>();

			_logger.LogInformation("Unsubscribing from event {EventName}", eventName);

			_store.RemoveSubscription<TEvent, TEventHandler>();
		}
	}
}