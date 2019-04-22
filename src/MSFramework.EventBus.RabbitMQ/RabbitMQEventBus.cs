using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MSFramework.EventBus.RabbitMQ
{
	public class RabbitMQEventBus : IDistributedEventBus, IDisposable
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
			IServiceScopeFactory serviceScopeFactory, IEventBusSubscriptionStore subsManager, string queueName = null,
			int retryCount = 5)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_store = subsManager ?? new InMemoryEventBusSubscriptionStore();
			_queueName = queueName;
			_consumerChannel = CreateConsumerChannel();
			_serviceScopeFactory = serviceScopeFactory;
			_retryCount = retryCount;
			_store.OnEventRemoved += EventBusStore_OnEventRemoved;
			_options = options;
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

				var message = JsonConvert.SerializeObject(@event);
				var body = Encoding.UTF8.GetBytes(message);

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

		public void Subscribe<T, TH>() where T : class, IEvent where TH : IEventHandler<T>
		{
			var eventName = _store.GetEventKey<T>();
			Subscribe(eventName);

			_logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName,
				typeof(TH).GetEventName());

			_store.AddSubscription<T, TH>();
			StartBasicConsume();
		}

		public void Subscribe<TH>(string eventName) where TH : IDynamicEventHandler
		{
			_logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName,
				typeof(TH).GetEventName());

			Subscribe(eventName);
			_store.AddSubscription<TH>(eventName);
			StartBasicConsume();
		}

		public void Unsubscribe<TH>(string eventName) where TH : IDynamicEventHandler
		{
			_store.RemoveSubscription<TH>(eventName);
		}

		public void Unsubscribe<T, TH>() where T : class, IEvent where TH : IEventHandler<T>
		{
			_store.RemoveSubscription<T, TH>();
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
			var message = Encoding.UTF8.GetString(eventArgs.Body);

			try
			{
				if (message.ToLowerInvariant().Contains("throw-fake-exception"))
				{
					throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
				}

				await ProcessEvent(eventName, message);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
			}

			// Even on exception we take the message off the queue.
			// in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
			// For more information see: https://www.rabbitmq.com/dlx.html
			_consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
		}

		private async Task ProcessEvent(string eventName, string message)
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
							dynamic eventData = JObject.Parse(message);
							await handler.Handle(eventData);
						}
						else
						{
							var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
							if (handler == null) continue;
							var eventType = _store.GetEventType(eventName);
							var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
							var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
							// ReSharper disable once PossibleNullReferenceException
							await (Task) concreteType.GetMethod("Handle").Invoke(handler, new[] {integrationEvent});
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