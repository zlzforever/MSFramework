using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MicroserviceFramework.RabbitMQ
{
	public class RabbitMqEventBus : IEventBus
	{
		private readonly RabbitMqOptions _options;
		private readonly PersistentConnection _connection;
		private IModel _consumerChannel;
		private readonly ILogger _logger;
		private readonly ISerializer _serializer;
		private readonly IEventHandlerFactory _eventHandlerFactory;

		public RabbitMqEventBus(RabbitMqOptions options, IConnectionFactory connectionFactory,
			IEventHandlerFactory eventHandlerFactory,
			ILoggerFactory loggerFactory, ISerializer serializer)
		{
			_options = options;
			_logger = loggerFactory.CreateLogger<RabbitMqEventBus>();
			_serializer = serializer;
			_eventHandlerFactory = eventHandlerFactory;
			_connection = new PersistentConnection(connectionFactory,
				loggerFactory.CreateLogger<PersistentConnection>(), _options.RetryCount);
			_consumerChannel = CreateConsumerChannel();
			
			SubscribeAllEventTypes();
			StartConsume();
		}

		public async Task PublishAsync(EventBase @event)
		{
			Check.NotNull(@event, nameof(@event));

			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			var policy = Policy.Handle<BrokerUnreachableException>()
				.Or<SocketException>()
				.WaitAndRetry(_options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					(ex, time) =>
					{
						_logger.LogWarning(ex,
							"Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})",
							@event.EventId,
							$"{time.TotalSeconds:n1}", ex.Message);
					});

			var eventName = @event.GetType().Name;
			var channel = _connection.CreateModel();

			_logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.EventId);

			channel.ExchangeDeclare(exchange: _options.Exchange, type: "direct");

			var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(@event));

			policy.Execute(() =>
			{
				var properties = channel.CreateBasicProperties();
				properties.DeliveryMode = 2; // persistent

				_logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.EventId);

				channel.BasicPublish(_options.Exchange, eventName, true, properties, bytes);
				channel.Dispose();
			});

			await Task.Yield();
		}

		private IModel CreateConsumerChannel()
		{
			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			_logger.LogTrace("Creating RabbitMQ consumer channel");

			var channel = _connection.CreateModel();
			channel.ExchangeDeclare(exchange: _options.Exchange, "direct");
			channel.QueueDeclare(queue: _options.Queue, true, false, false, null);
			channel.CallbackException += (sender, ea) =>
			{
				_logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

				_consumerChannel.Dispose();
				_consumerChannel = CreateConsumerChannel();
				
				StartConsume();
			};

			return channel;
		}

		private void StartConsume()
		{
			_logger.LogTrace("Starting RabbitMQ basic consume");

			if (_consumerChannel != null)
			{
				var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

				consumer.Received += async (sender, args) =>
				{
					var message = Encoding.UTF8.GetString(args.Body.ToArray());

					try
					{
						var eventName = args.RoutingKey;
						var handlerInfos = EventHandlerTypeCache.GetOrDefault(eventName);
						foreach (var handlerInfo in handlerInfos)
						{
							var @event = _serializer.Deserialize(message, handlerInfo.EventType);
							var handlers = _eventHandlerFactory.Create(handlerInfo.HandlerType);
							foreach (var handler in handlers)
							{
								await Task.Yield();
								await ((Task) handlerInfo.MethodInfo.Invoke(handler, new[] {@event})).ConfigureAwait(
									false);
							}
						}
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Processing message failed: \"{Message}\"", message);
					}

					// Even on exception we take the message off the queue.
					// in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
					// For more information see: https://www.rabbitmq.com/dlx.html
					_consumerChannel.BasicAck(args.DeliveryTag, multiple: false);
				};

				_consumerChannel.BasicConsume(_options.Queue, false, consumer);
			}
			else
			{
				_logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
			}
		}

		private void SubscribeAllEventTypes()
		{
			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			foreach (var eventType in EventHandlerTypeCache.GetEventTypes())
			{
				var eventName = eventType.Name;
				using var channel = _connection.CreateModel();
				channel.QueueBind(_options.Queue, _options.Exchange, eventName);
			}
		}

		public void Dispose()
		{
			_connection.Dispose();
		}
	}
}