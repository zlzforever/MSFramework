// using System;
// using System.Net.Sockets;
// using System.Threading.Tasks;
// using MessagePack;
// using MicroserviceFramework.Domain.Event;
// using MicroserviceFramework.EventBus;
// using Microsoft.Extensions.Logging;
// using Polly;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;
// using RabbitMQ.Client.Exceptions;
// using IHandlerFactory = MicroserviceFramework.Domain.Event.IHandlerFactory;
//
// namespace MicroserviceFramework.RabbitMQ
// {
// 	public class RabbitMqEventDispatcher : ChannelEventBus
// 	{
// 		private readonly RabbitMqOptions _options;
// 		private readonly PersistentConnection _connection;
// 		private IModel _consumerChannel;
// 		private readonly ILogger _logger;
//
// 		public RabbitMqEventDispatcher(RabbitMqOptions options,
// 			IEventHandlerTypeStore eventHandlerTypeStore, IHandlerFactory handlerFactory, ILoggerFactory loggerFactory)
// 			: base(eventHandlerTypeStore, handlerFactory)
// 		{
// 			_logger = loggerFactory.CreateLogger<RabbitMqEventDispatcher>();
// 			_options = options;
// 			_connection = new PersistentConnection(CreateConnectionFactory(),
// 				loggerFactory.CreateLogger<PersistentConnection>(), _options.RetryCount);
// 			_consumerChannel = CreateConsumerChannel();
// 		}
//
// 		public override bool Register(Type eventType, Type handlerType)
// 		{
// 			var registered = base.Register(eventType, handlerType);
// 			if (registered)
// 			{
// 				var eventName = GenerateEventName(eventType);
// 				BindQueue(eventName);
// 				return true;
// 			}
//
// 			return false;
// 		}
//
// 		public override async Task DispatchAsync(IEvent @event)
// 		{
// 			if (@event == null)
// 			{
// 				throw new ArgumentNullException(nameof(@event));
// 			}
//
// 			if (@event is IntegrationEvent)
// 			{
// 				if (!_connection.IsConnected)
// 				{
// 					_connection.TryConnect();
// 				}
//
// 				var policy = Policy.Handle<BrokerUnreachableException>()
// 					.Or<SocketException>()
// 					.WaitAndRetry(_options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
// 						(ex, time) =>
// 						{
// 							_logger.LogWarning(ex,
// 								"Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})",
// 								@event.EventId,
// 								$"{time.TotalSeconds:n1}", ex.Message);
// 						});
// 				var eventType = @event.GetType();
// 				var eventName = GenerateEventName(eventType);
// 				var channel = _connection.CreateModel();
//
// 				_logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.EventId);
//
// 				channel.ExchangeDeclare(exchange: _options.Exchange, type: "direct");
//
// 				var bytes = MessagePackSerializer.Typeless.Serialize(@event);
//
// 				policy.Execute(() =>
// 				{
// 					var properties = channel.CreateBasicProperties();
// 					properties.DeliveryMode = 2; // persistent
//
// 					_logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.EventId);
//
// 					channel.BasicPublish(_options.Exchange, eventName, true, properties, bytes);
// 					channel.Dispose();
// 				});
// 			}
// 			else
// 			{
// 				await base.DispatchAsync(@event);
// 			}
// 		}
//
// 		private IConnectionFactory CreateConnectionFactory()
// 		{
// 			var connectionFactory = new ConnectionFactory
// 			{
// 				HostName = _options.HostName,
// 				DispatchConsumersAsync = true
// 			};
// 			if (_options.Port > 0)
// 			{
// 				connectionFactory.Port = _options.Port;
// 			}
//
// 			if (!string.IsNullOrWhiteSpace(_options.UserName))
// 			{
// 				connectionFactory.UserName = _options.UserName;
// 			}
//
// 			if (!string.IsNullOrWhiteSpace(_options.Password))
// 			{
// 				connectionFactory.Password = _options.Password;
// 			}
//
// 			return connectionFactory;
// 		}
//
// 		private IModel CreateConsumerChannel()
// 		{
// 			if (!_connection.IsConnected)
// 			{
// 				_connection.TryConnect();
// 			}
//
// 			_logger.LogTrace("Creating RabbitMQ consumer channel");
//
// 			var channel = _connection.CreateModel();
// 			channel.ExchangeDeclare(exchange: _options.Exchange, "direct");
// 			channel.QueueDeclare(queue: _options.Queue, true, false, false, null);
// 			channel.CallbackException += (sender, ea) =>
// 			{
// 				_logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");
//
// 				_consumerChannel.Dispose();
// 				_consumerChannel = CreateConsumerChannel();
// 				StartConsume();
// 			};
//
// 			return channel;
// 		}
//
// 		private void StartConsume()
// 		{
// 			_logger.LogTrace("Starting RabbitMQ basic consume");
//
// 			if (_consumerChannel != null)
// 			{
// 				var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
//
// 				consumer.Received += async (sender, args) =>
// 				{
// 					var bytes = args.Body.ToArray();
// 					var @event = MessagePackSerializer.Typeless.Deserialize(bytes) as Intergr;
// 					await base.DispatchAsync(@event);
//
// 					// Even on exception we take the message off the queue.
// 					// in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
// 					// For more information see: https://www.rabbitmq.com/dlx.html
// 					_consumerChannel.BasicAck(args.DeliveryTag, multiple: false);
// 				};
//
// 				_consumerChannel.BasicConsume(_options.Queue, false, consumer);
// 			}
// 			else
// 			{
// 				_logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
// 			}
// 		}
//
// 		private string GenerateEventName(Type type)
// 		{
// 			return type.FullName;
// 		}
//
// 		private void BindQueue(string eventName)
// 		{
// 			if (!_connection.IsConnected)
// 			{
// 				_connection.TryConnect();
// 			}
//
// 			using var channel = _connection.CreateModel();
// 			channel.QueueBind(_options.Queue, _options.Exchange, eventName);
// 		}
//
// 		public override void Dispose()
// 		{
// 			base.Dispose();
//
// 			_connection.Dispose();
// 		}
// 	}
// }