using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.EventBus;

namespace MSFramework.CQRS
{
	public class CommandBus : ICommandBus
	{
		private readonly Dictionary<string, Type> _handlers = new Dictionary<string, Type>();

		private readonly ILogger _logger;
		private readonly IServiceProvider _scopeFactory;

		public CommandBus(ILogger<CommandBus> logger, IServiceProvider scopeFactory)
		{
			_logger = logger;
			_scopeFactory = scopeFactory;
		}

		public async Task PublishAsync(ICommand @event)
		{
			var command = @event.GetType().FullName;
			if (string.IsNullOrEmpty(command))
			{
				throw new MSFrameworkException("Command is empty/null");
			}

			if (_handlers.ContainsKey(command))
			{
				var handlerType = _handlers[command];
				var handler = _scopeFactory.GetService(handlerType);
				if (handler == null) return;

				var concreteType = typeof(ICommandHandler<>).MakeGenericType(@event.GetType());
				// ReSharper disable once PossibleNullReferenceException
				await (Task) concreteType.GetMethod("Handle").Invoke(handler, new object[] {@event});
			}
		}

		public void Subscribe<T, TH>() where T : class, ICommand where TH : ICommandHandler<T>
		{
			var command = typeof(T).FullName;

			if (string.IsNullOrEmpty(command))
			{
				throw new MSFrameworkException("Command is empty/null");
			}

			_logger.LogInformation("Subscribing to command {command} with {CommandHandler}", command,
				typeof(TH).GetEventName());

			if (_handlers.ContainsKey(command))
			{
				_handlers[command] = typeof(TH);
			}
			else
			{
				_handlers.Add(command, typeof(TH));
			}
		}

		public void Unsubscribe<T, TH>() where T : class, ICommand where TH : ICommandHandler<T>
		{
			var command = typeof(T).FullName;

			if (string.IsNullOrEmpty(command))
			{
				throw new MSFrameworkException("Command is empty/null");
			}

			if (_handlers.ContainsKey(command))
			{
				_handlers.Remove(command);
			}
		}
	}
}