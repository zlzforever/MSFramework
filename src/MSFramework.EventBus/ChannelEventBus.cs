using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;
using MicroserviceFramework.Serializer;
using Microsoft.Extensions.Logging;


namespace MicroserviceFramework.EventBus
{
	public class ChannelEventBus : IEventBus
	{
		private readonly ISubscriptionInfoStore _subscriptionInfoStore;
		private readonly IEventHandlerFactory _handlerFactory;
		private readonly ILogger<ChannelEventBus> _logger;

		private readonly ConcurrentDictionary<string, Channel<string>> _channelDict =
			new ConcurrentDictionary<string, Channel<string>>();

		private readonly ISerializer _serializer;

		public ChannelEventBus(ISubscriptionInfoStore subscriptionInfoStore,
			ISerializer serializer,
			IEventHandlerFactory handlerFactory, ILogger<ChannelEventBus> logger)
		{
			_subscriptionInfoStore = subscriptionInfoStore;
			_serializer = serializer;
			_handlerFactory = handlerFactory;
			_logger = logger;
		}

		public virtual Task PublishAsync(Event @event)
		{
			var eventName = @event.GetType().Name;
			if (_channelDict.TryGetValue(eventName, out var channel))
			{
				channel.Writer.TryWrite(_serializer.Serialize(@event));
			}

			return Task.CompletedTask;
		}

		public virtual async Task SubscribeAsync<T, TH>()
			where T : Event where TH : IEventHandler<T>
		{
			await SubscribeAsync(typeof(T), typeof(TH));
		}

		public virtual async Task SubscribeAsync(Type eventType, Type handlerType)
		{
			if (!eventType.IsEvent())
			{
				throw new EventBusException($"{eventType} 不是一个事件");
			}

			if (!handlerType.CanHandle(eventType))
			{
				throw new EventBusException($"{handlerType} 不能处理 {eventType}");
			}

			var eventName = _subscriptionInfoStore.GetEventKey(eventType);

			var channel = Channel.CreateUnbounded<string>();

			if (_channelDict.TryAdd(eventName, channel))
			{
				await Task.Factory.StartNew(async () =>
				{
					while (await channel.Reader.WaitToReadAsync())
					{
						var json = await channel.Reader.ReadAsync();
						Task.Factory.StartNew(async () => { await HandleEventAsync(eventName, json); })
							.ConfigureAwait(false).GetAwaiter();
					}
				});
			}

			_subscriptionInfoStore.Add(eventType, handlerType);
		}

		public virtual void Unsubscribe<TEvent, TEventHandler>()
			where TEvent : Event where TEventHandler : IEventHandler<TEvent>
		{
			_subscriptionInfoStore.Remove<TEvent, TEventHandler>();
			var eventName = _subscriptionInfoStore.GetEventKey<TEvent>();
			if (_subscriptionInfoStore.GetHandlers(eventName).Count == 0 &&
			    _channelDict.TryGetValue(eventName, out var channel))
			{
				channel.Writer.Complete();
			}
		}

		private async Task HandleEventAsync(string eventName, string json)
		{
			try
			{
				var subscriptionInfos = _subscriptionInfoStore.GetHandlers(eventName);
				foreach (var subscriptionInfo in subscriptionInfos)
				{
					var @event = _serializer.Deserialize(json, subscriptionInfo.EventType);
					if (@event == null)
					{
						_logger.LogWarning(
							$"Create event {subscriptionInfo.EventType} failed");
						continue;
					}

					var handler = _handlerFactory.Create(subscriptionInfo.HandlerType);
					if (handler != null)
					{
						if (subscriptionInfo.Method.Invoke(handler, new[] {@event}) is Task task)
						{
							await task;
						}
					}
					else
					{
						_logger.LogWarning(
							$"Create handler {subscriptionInfo.HandlerType} failed");
					}
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e.ToString());
			}
		}
	}
}