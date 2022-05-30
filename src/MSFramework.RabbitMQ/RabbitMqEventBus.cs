using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.DependencyInjection;
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
		private readonly IServiceProvider _serviceProvider;

		public RabbitMqEventBus(RabbitMqOptions options, IConnectionFactory connectionFactory,
			IServiceProvider serviceProvider,
			ILoggerFactory loggerFactory, ISerializer serializer)
		{
			_options = options;
			_logger = loggerFactory.CreateLogger<RabbitMqEventBus>();
			_serializer = serializer;
			_serviceProvider = serviceProvider;
			_connection = new PersistentConnection(connectionFactory,
				loggerFactory.CreateLogger<PersistentConnection>(), _options.RetryCount);
			_consumerChannel = CreateConsumerChannel();

			SubscribeAllEventTypes();
			StartConsume();
		}

		public async Task PublishAsync(dynamic @event)
		{
			Check.NotNull(@event, nameof(@event));

			var type = @event.GetType();
			if (!type.IsEvent())
			{
				throw new MicroserviceFrameworkException($"类型 {type} 不是事件");
			}

			var eventKey = GetEventKey(type);
			await PublishAsync(eventKey, @event);
		}

		protected virtual string GetEventKey(Type type)
		{
			return type.Name;
		}

		public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
		{
			Check.NotNull(@event, nameof(@event));
			var eventKey = GetEventKey(typeof(TEvent));
			await PublishAsync(eventKey, @event);
		}

		public async Task<bool> PublishIfEventAsync(dynamic @event)
		{
			if (@event == null)
			{
				return false;
			}

			var type = (Type)@event.GetType();
			if (!type.IsEvent())
			{
				return false;
			}

			var eventKey = GetEventKey(type);
			await PublishAsync(eventKey, @event);
			return true;
		}

		private async Task PublishAsync(string eventKey, dynamic @event)
		{
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
							$"Could not publish event: {@event.EventId} after {time.TotalSeconds:n1}s ({ex.Message})");
					});

			var channel = _connection.CreateModel();

			_logger.LogTrace($"Declaring RabbitMQ exchange to publish event: {@event.EventId}");

			channel.ExchangeDeclare(_options.Exchange, "direct");

			var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(@event));

			policy.Execute(() =>
			{
				var properties = channel.CreateBasicProperties();
				properties.DeliveryMode = 2; // persistent

				_logger.LogTrace($"Publishing event to RabbitMQ: {@event.EventId}");

				channel.BasicPublish(_options.Exchange, eventKey, true, properties, bytes);
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
			channel.ExchangeDeclare(_options.Exchange, "direct");
			channel.QueueDeclare(_options.Queue, true, false, false, null);
			channel.CallbackException += (_, ea) =>
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

				consumer.Received += async (_, args) =>
				{
					var message = Encoding.UTF8.GetString(args.Body.ToArray());

					try
					{
						var eventName = args.RoutingKey;
						var handlerInfos = EventHandlerTypeCache.GetOrDefault(eventName);
						foreach (var handlerInfo in handlerInfos)
						{
							var @event = _serializer.Deserialize(message, handlerInfo.Value.EventType);
							await using var scope = _serviceProvider.CreateAsyncScope();
							var handlers = scope.ServiceProvider.GetServices(handlerInfo.Key);
							foreach (var handler in handlers)
							{
								if (handler == null)
								{
									continue;
								}

								await Task.Yield();

								var task = handlerInfo.Value.MethodInfo.Invoke(handler, new[] { @event }) as Task;
								if (task != null)
								{
									await task.ConfigureAwait(false);
								}
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
					_consumerChannel.BasicAck(args.DeliveryTag, false);
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