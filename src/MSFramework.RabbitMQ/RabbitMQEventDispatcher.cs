using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MessagePack;
using MSFramework.Domain.Event;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MSFramework.RabbitMQ
{
	public class RabbitMQEventDispatcher : EventDispatcher
	{
		private readonly IConnection _connection;
		private readonly ConcurrentDictionary<string, IModel> _modelDict;
		private readonly RabbitMQOptions _options;

		public RabbitMQEventDispatcher(RabbitMQOptions options, IEventHandlerTypeStore eventHandlerTypeStore,
			IHandlerFactory handlerFactory) : base(eventHandlerTypeStore, handlerFactory)

		{
			_options = options;
			_modelDict = new ConcurrentDictionary<string, IModel>();

			var connectionFactory = new ConnectionFactory
			{
				HostName = _options.HostName,

				DispatchConsumersAsync = true
			};
			if (_options.Port > 0)
			{
				connectionFactory.Port = _options.Port;
			}

			if (!string.IsNullOrWhiteSpace(_options.UserName))
			{
				connectionFactory.UserName = _options.UserName;
			}

			if (!string.IsNullOrWhiteSpace(_options.Password))
			{
				connectionFactory.Password = _options.Password;
			}

			_connection = connectionFactory.CreateConnection();
		}

		public override bool Register(Type eventType, Type handlerType)
		{
			var registered = base.Register(eventType, handlerType);
			if (registered)
			{
				var topic = GenerateTopic(eventType);

				return RegisterRabbitMq(topic);
			}

			return false;
		}

		public override async Task DispatchAsync(IEvent @event)
		{
			if (@event == null)
			{
				throw new ArgumentNullException(nameof(@event));
			}

			if (@event is IIntegrationEvent)
			{
				var eventType = @event.GetType();
				var topic = GenerateTopic(eventType);
				if (_modelDict.TryGetValue(topic, out var channel))
				{
					var bytes = MessagePackSerializer.Typeless.Serialize(@event);
					channel.BasicPublish(_options.Exchange, topic, null, bytes);
				}
				else
				{
					throw new ApplicationException("Get channel failed");
				}
			}
			else
			{
				await base.DispatchAsync(@event);
			}
		}

		private string GenerateTopic(Type type)
		{
			return type.FullName;
		}

		private bool RegisterRabbitMq(string topic)
		{
			lock (_modelDict)
			{
				if (!_modelDict.ContainsKey(topic))
				{
					var channel = _connection.CreateModel();
					channel.QueueDeclare(topic, durable: true, exclusive: false, autoDelete: false,
						arguments: null);
					channel.ExchangeDeclare(exchange: _options.Exchange, type: "direct", durable: true);

					var queue = channel.QueueDeclare().QueueName;
					channel.QueueBind(queue: queue, _options.Exchange, routingKey: topic);

					var consumer = new AsyncEventingBasicConsumer(channel);

					consumer.Received += async (model, ea) =>
					{
						var bytes = ea.Body.ToArray();
						var obj = MessagePackSerializer.Typeless.Deserialize(bytes) as IEvent;
						await base.DispatchAsync(obj);
						channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
					};

					//7. 启动消费者
					channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);

					return _modelDict.TryAdd(topic, channel);
				}

				return true;
			}
		}
	}
}