// using System;
// using System.Net.Sockets;
// using System.Text;
// using System.Threading.Tasks;
// using MicroserviceFramework.Utilities;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Polly;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;
// using RabbitMQ.Client.Exceptions;
//
// namespace MicroserviceFramework.EventBus.RabbitMQ;
//
// public class EventBusRabbitMQ : IEventBus
// {
//     private readonly RabbitMQOptions _options;
//     private readonly PersistentConnection _connection;
//     private IModel _consumerChannel;
//     private readonly ILogger _logger;
//     private readonly IEventExecutor _eventExecutor;
//
//     public EventBusRabbitMQ(IOptionsMonitor<RabbitMQOptions> options,
//         PersistentConnection connection,
//         IEventExecutor eventExecutor, ILogger<EventBusRabbitMQ> logger
//     )
//     {
//         _options = options.CurrentValue;
//         _logger = logger;
//         _eventExecutor = eventExecutor;
//         _connection = connection;
//         _consumerChannel = CreateConsumerChannel();
//     }
//
//     public void SubscribeAllEventTypes()
//     {
//         foreach (var eventName in EventSubscriptionManager.GetEventTypes())
//         {
//             Subscribe(eventName);
//         }
//
//         StartConsume();
//     }
//
//     public Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
//     {
//         Check.NotNull(@event, nameof(@event));
//
//         if (!_connection.IsConnected)
//         {
//             _connection.TryConnect();
//         }
//
//         var policy = Policy.Handle<BrokerUnreachableException>()
//             .Or<SocketException>()
//             .WaitAndRetry(_options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
//                 (ex, time) =>
//                 {
//                     _logger.LogWarning(ex,
//                         $"Could not publish event: {@event.EventId} after {time.TotalSeconds:n1}s ({ex.Message})");
//                 });
//
//         var eventName = @event.GetType().GetEventName();
//
//         using var channel = _connection.CreateModel();
//         _logger.LogTrace($"Declaring RabbitMQ exchange to publish event: {@event.EventId}");
//         channel.ExchangeDeclare(_options.Exchange, "direct");
//
//         var bytes = Encoding.UTF8.GetBytes(Defaults.JsonHelper.Serialize(@event));
//
//         policy.Execute(() =>
//         {
//             var properties = channel.CreateBasicProperties();
//             properties.DeliveryMode = 2; // persistent
//
//             _logger.LogTrace($"Publishing event to RabbitMQ: {@event.EventId}");
//
//             channel.BasicPublish(_options.Exchange, eventName, true, properties, bytes);
//         });
//
//         return Task.CompletedTask;
//     }
//
//     public void AddInterceptors(InterceptorType type, params Func<IServiceProvider, Task>[] funcs)
//     {
//     }
//
//     private void Subscribe(string eventName)
//     {
//         if (!_connection.IsConnected)
//         {
//             _connection.TryConnect();
//         }
//
//         _consumerChannel.QueueBind(queue: _options.Queue,
//             exchange: _options.Exchange,
//             routingKey: eventName);
//     }
//
//     private IModel CreateConsumerChannel()
//     {
//         if (!_connection.IsConnected)
//         {
//             _connection.TryConnect();
//         }
//
//         _logger.LogTrace("Creating RabbitMQ consumer channel");
//
//         var channel = _connection.CreateModel();
//         channel.ExchangeDeclare(_options.Exchange, "direct");
//         channel.QueueDeclare(_options.Queue, true, false, false, null);
//         channel.CallbackException += (_, ea) =>
//         {
//             _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");
//
//             _consumerChannel.Dispose();
//             _consumerChannel = CreateConsumerChannel();
//
//             StartConsume();
//         };
//
//         return channel;
//     }
//
//     private async Task EventReceived(object sender, BasicDeliverEventArgs args)
//     {
//         var message = Encoding.UTF8.GetString(args.Body.ToArray());
//         try
//         {
//             var eventName = args.RoutingKey;
//             await _eventExecutor.ExecuteAsync(eventName, message);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogWarning(ex, "Processing message failed: \"{Message}\"", message);
//         }
//
//         // Even on exception we take the message off the queue.
//         // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
//         // For more information see: https://www.rabbitmq.com/dlx.html
//         _consumerChannel.BasicAck(args.DeliveryTag, false);
//     }
//
//     private void StartConsume()
//     {
//         _logger.LogTrace("Starting RabbitMQ basic consume");
//
//         if (_consumerChannel != null)
//         {
//             var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
//             consumer.Received += EventReceived;
//             _consumerChannel.BasicConsume(_options.Queue, false, consumer);
//         }
//         else
//         {
//             _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
//         }
//     }
//
//     // private void OnEventRemoved(object sender, string eventName)
//     // {
//     // 	if (!_connection.IsConnected)
//     // 	{
//     // 		_connection.TryConnect();
//     // 	}
//     //
//     // 	using var channel = _connection.CreateModel();
//     // 	channel.QueueUnbind(queue: _options.Queue, exchange: _options.Exchange, routingKey: eventName);
//     //
//     // 	if (EventSubscriptionManager.IsEmpty)
//     // 	{
//     // 		_consumerChannel.Close();
//     // 	}
//     // }
//
//     public void Dispose()
//     {
//         _consumerChannel.Dispose();
//         _connection.Dispose();
//     }
// }
