using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Application
{
	public class DefaultCommandExecutor : ICommandExecutor
	{
		private readonly CommandHandlerTypeCache _cache;
		private readonly IServiceProvider _serviceProvider;

		public DefaultCommandExecutor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_cache = _serviceProvider.GetRequiredService<CommandHandlerTypeCache>();
		}

		public void Register(Type commandType, Type handlerType)
		{
			if (commandType == null)
			{
				throw new ArgumentNullException(nameof(commandType));
			}

			if (handlerType == null)
			{
				throw new ArgumentNullException(nameof(handlerType));
			}

			if (!typeof(ICommand).IsAssignableFrom(commandType))
			{
				throw new ArgumentException("Command should inherit from ICommand or ICommand<T>");
			}

			var commandType1 = handlerType.GetInterface("ICommandHandler`1")?.GenericTypeArguments
				.FirstOrDefault();
			if (commandType1 != null && commandType1 == commandType)
			{
				if (_cache.Contains(commandType))
				{
					throw new ArgumentException(
						$"There are more than 1 command handler for command: [{commandType.FullName}]");
				}

				_cache.TryAdd(commandType, handlerType);
			}
			else
			{
				throw new ArgumentException($"Type {handlerType} is not a valid command handler");
			}
		}

		public async Task ExecuteAsync(ICommand command, CancellationToken cancellationToken)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			var commandType = command.GetType();
			if (_cache.TryGetHandlerType(commandType, out (Type Type, MethodInfo Method) tuple))
			{
				var handler = _serviceProvider.GetRequiredService(tuple.Type);
				await (Task) tuple.Method.Invoke(handler, new object[] {command, cancellationToken});
			}
			else
			{
				throw new ApplicationException("找不到命令处理器");
			}
		}

		public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command,
			CancellationToken cancellationToken)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			var commandType = command.GetType();
			if (_cache.TryGetHandlerType(commandType, out (Type Type, MethodInfo Method) tuple))
			{
				var handler = _serviceProvider.GetRequiredService(tuple.Type);
				return await (Task<TResult>) tuple.Method.Invoke(handler, new object[] {command, cancellationToken});
			}
			else
			{
				throw new ApplicationException("找不到命令处理器");
			}
		}
	}
}