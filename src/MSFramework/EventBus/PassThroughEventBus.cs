using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MSFramework.EventBus
{
	public class PassThroughEventBus : IEventBus
	{
		private static readonly IEventBusSubscriptionStore Store;
		private readonly ILogger _logger;
		private readonly IServiceProvider _serviceProvider;

		static PassThroughEventBus()
		{
			Store = new InMemoryEventBusSubscriptionStore();
		}
		
		public PassThroughEventBus(ILogger<PassThroughEventBus> logger, IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		public async Task PublishAsync(IEvent @event)
		{
			var eventName = Store.GetEventKey(@event.GetType());
			if (Store.ContainSubscription(eventName))
			{
				var subscriptions = Store.GetHandlers(eventName);
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
			var eventName = Store.GetEventKey<TEvent>();

			_logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName,
				typeof(TEventHandler).GetEventName());

			Store.AddSubscription<TEvent, TEventHandler>();
		}

		public void Subscribe<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			_logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName,
				typeof(TEventHandler).GetEventName());
			Store.AddSubscription<TEventHandler>(eventName);
		}

		public void Unsubscribe<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			Store.RemoveSubscription<TEventHandler>(eventName);
		}

		public void Unsubscribe<TEvent, TEventHandler>() where TEvent : class, IEvent where TEventHandler : IEventHandler<TEvent>
		{
			var eventName = Store.GetEventKey<TEvent>();

			_logger.LogInformation("Un-subscribing from event {EventName}", eventName);

			Store.RemoveSubscription<TEvent, TEventHandler>();
		}
	}
}