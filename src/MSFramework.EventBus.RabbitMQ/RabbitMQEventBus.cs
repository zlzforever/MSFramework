using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MSFramework.EventBus.RabbitMQ
{
	public class RabbitMQEventBus : IEventBus, IDisposable
	{
		private readonly RabbitMQConnection _connection;
		private readonly ILogger<RabbitMQEventBus> _logger;
		private readonly IEventBusSubscriptionStore _store;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly int _retryCount;
		private readonly RabbitMQOptions _options;
		private string _queueName;
		private IModel _consumerChannel;

		public RabbitMQEventBus(RabbitMQConnection connection,
			RabbitMQOptions options,
			ILogger<RabbitMQEventBus> logger,
			IServiceScopeFactory serviceScopeFactory, IEventBusSubscriptionStore subsManager,
			string queueName = "RabbitMQEventBus",
			int retryCount = 5)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_store = subsManager ?? new InMemoryEventBusSubscriptionStore();
			_queueName = queueName;
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
			_retryCount = retryCount;
			_store.OnEventRemoved += EventBusStore_OnEventRemoved;
			_consumerChannel = CreateConsumerChannel();
		}

		public Task PublishAsync(IEvent @event)
		{
			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			var policy = Policy.Handle<BrokerUnreachableException>()
				.Or<SocketException>()
				.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					(ex, time) =>
					{
						_logger.LogWarning(ex,
							"Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id,
							$"{time.TotalSeconds:n1}", ex.Message);
					});

			using (var channel = _connection.CreateModel())
			{
				var eventName = @event.GetType()
					.Name;

				channel.ExchangeDeclare(exchange: _options.BrokerName,
					type: "direct");

				var body = MessagePackSerializer.Typeless.Serialize(@event);

				policy.Execute(() =>
				{
					var properties = channel.CreateBasicProperties();
					properties.DeliveryMode = 2; // persistent

					channel.BasicPublish(exchange: _options.BrokerName,
						routingKey: eventName,
						mandatory: true,
						basicProperties: properties,
						body: body);
				});
			}

			return Task.CompletedTask;
		}

		public void Subscribe<TEvent, TEventHandler>() where TEvent : class, IEvent
			where TEventHandler : IEventHandler<TEvent>
		{
			var eventName = _store.GetEventKey<TEvent>();
			Subscribe(eventName);

			_logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName,
				typeof(TEventHandler).GetEventName());

			_store.AddSubscription<TEvent, TEventHandler>();
			StartBasicConsume();
		}

		public void Subscribe<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			_logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName,
				typeof(TEventHandler).GetEventName());

			Subscribe(eventName);
			_store.AddSubscription<TEventHandler>(eventName);
			StartBasicConsume();
		}

		public void Unsubscribe<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			_store.RemoveSubscription<TEventHandler>(eventName);
		}

		public void Unsubscribe<TEvent, TEventHandler>() where TEvent : class, IEvent
			where TEventHandler : IEventHandler<TEvent>
		{
			_store.RemoveSubscription<TEvent, TEventHandler>();
		}

		public void Dispose()
		{
			_consumerChannel?.Dispose();

			_store.Clear();
		}

		private IModel CreateConsumerChannel()
		{
			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			var channel = _connection.CreateModel();

			channel.ExchangeDeclare(exchange: _options.BrokerName,
				type: "direct");

			channel.QueueDeclare(queue: _queueName,
				durable: true,
				exclusive: false,
				autoDelete: false,
				arguments: null);

			channel.CallbackException += (sender, ea) =>
			{
				_consumerChannel.Dispose();
				_consumerChannel = CreateConsumerChannel();
				StartBasicConsume();
			};

			return channel;
		}

		private void StartBasicConsume()
		{
			if (_consumerChannel != null)
			{
				var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

				consumer.Received += Consumer_Received;

				_consumerChannel.BasicConsume(
					queue: _queueName,
					autoAck: false,
					consumer: consumer);
			}
			else
			{
				_logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
			}
		}

		private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
		{
			var eventName = eventArgs.RoutingKey;
			var @event =
				MessagePackSerializer.Typeless.Deserialize(eventArgs.Body);
			await ProcessEvent(eventName, @event);

			// Even on exception we take the message off the queue.
			// in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
			// For more information see: https://www.rabbitmq.com/dlx.html
			_consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
		}

		private async Task ProcessEvent(string eventName, object @event)
		{
			if (_store.ContainSubscription(eventName))
			{
				using (var scope = _serviceScopeFactory.CreateScope())
				{
					var subscriptions = _store.GetHandlers(eventName);
					foreach (var subscription in subscriptions)
					{
						if (subscription.IsDynamic)
						{
							IDynamicEventHandler handler =
								scope.ServiceProvider.GetService(subscription.HandlerType) as IDynamicEventHandler;
							if (handler == null) continue;
							await handler.Handle(@event);
						}
						else
						{
							var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
							if (handler == null) continue;
							var eventType = _store.GetEventType(eventName);
							var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
							// ReSharper disable once PossibleNullReferenceException
							await (Task) concreteType.GetMethod("Handle").Invoke(handler, new[] {@event});
						}
					}
				}
			}
		}

		private void EventBusStore_OnEventRemoved(object sender, string eventName)
		{
			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			using (var channel = _connection.CreateModel())
			{
				channel.QueueUnbind(queue: _queueName,
					exchange: _options.BrokerName,
					routingKey: eventName);

				if (_store.IsEmpty)
				{
					_queueName = string.Empty;
					_consumerChannel.Close();
				}
			}
		}

		private void Subscribe(string eventName)
		{
			var containsKey = _store.ContainSubscription(eventName);
			if (!containsKey)
			{
				if (!_connection.IsConnected)
				{
					_connection.TryConnect();
				}

				using (var channel = _connection.CreateModel())
				{
					channel.QueueBind(queue: _queueName,
						exchange: _options.BrokerName,
						routingKey: eventName);
				}
			}
		}
	}
}