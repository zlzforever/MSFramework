using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Application.CQRS
{
	public class CqrsProcessor : ICqrsProcessor
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)> Cache;

		static CqrsProcessor()
		{
			Cache = new ConcurrentDictionary<Type, (Type, MethodInfo)>();
		}

		public CqrsProcessor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task QueryAsync(IQuery query, CancellationToken cancellationToken = default)
		{
			Check.NotNull(query, nameof(query));

			var queryType = query.GetType();

			var (@interface, method) = Cache.GetOrAdd(queryType, type => Create(typeof(IQueryHandler<>), type));

			var handler = _serviceProvider.GetService(@interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建查询处理器失败");
			}

			if (method.Invoke(handler, new object[] {query, cancellationToken}) is Task task)
			{
				await task;
			}
		}

		public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query,
			CancellationToken cancellationToken = default)
		{
			Check.NotNull(query, nameof(query));

			var queryType = query.GetType();
			var (@interface, method) =
				Cache.GetOrAdd(queryType, type => Create(typeof(IQueryHandler<,>), type, typeof(TResponse)));
			var handler = _serviceProvider.GetService(@interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建查询处理器失败");
			}

			if (method.Invoke(handler, new object[] {query, cancellationToken}) is Task<TResponse> task)
			{
				return await task;
			}

			return default;
		}

		public async Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			var commandType = command.GetType();
			var (@interface, method) = Cache.GetOrAdd(commandType, type => Create(typeof(ICommandHandler<>), type));
			var handler = _serviceProvider.GetService(@interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建命令处理器失败");
			}

			if (method.Invoke(handler, new object[] {command, cancellationToken}) is Task task)
			{
				await task;
			}
		}

		public async Task<TResponse> ExecuteAsync<TResponse>(ICommand<TResponse> command,
			CancellationToken cancellationToken = default)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			var commandType = command.GetType();
			var (@interface, method) = Cache.GetOrAdd(commandType,
				type => Create(typeof(ICommandHandler<,>), type, typeof(TResponse)));
			var handler = _serviceProvider.GetService(@interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建命令处理器失败");
			}

			if (method.Invoke(handler, new object[] {command, cancellationToken}) is Task<TResponse> task)
			{
				return await task;
			}

			return default;
		}

		private static (Type HandlerType, MethodInfo MethodInfo) Create(Type type, params Type[] typeArguments)
		{
			var handlerType = type.MakeGenericType(typeArguments);
			var method = handlerType.GetMethods()[0];
			return (handlerType, method);
		}
	}
}