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

			var type = query.GetType();
			var handlerInfo = Cache.GetOrAdd(type, x =>
			{
				var handlerType = typeof(IQueryHandler<>).MakeGenericType(x);
				var method = handlerType.GetMethods()[0];
				return (handlerType, method);
			});

			var handler = _serviceProvider.GetService(handlerInfo.Interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建处理器失败");
			}

			if (handlerInfo.Method.Invoke(handler, new object[] {query, cancellationToken}) is Task task)
			{
				await task;
			}
		}

		public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query,
			CancellationToken cancellationToken = default)
		{
			Check.NotNull(query, nameof(query));

			var type = query.GetType();
			var handlerInfo = Cache.GetOrAdd(type, x =>
			{
				var handlerType = typeof(IQueryHandler<,>).MakeGenericType(x, typeof(TResponse));
				var method = handlerType.GetMethods()[0];
				return (handlerType, method);
			});

			var handler = _serviceProvider.GetService(handlerInfo.Interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建处理器失败");
			}

			if (handlerInfo.Method.Invoke(handler, new object[] {query, cancellationToken}) is Task<TResponse> task)
			{
				return await task;
			}
			else
			{
				return default;
			}
		}

		public async Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			var type = command.GetType();
			var handlerInfo = Cache.GetOrAdd(type, x =>
			{
				var handlerType = typeof(ICommandHandler<>).MakeGenericType(x);
				var method = handlerType.GetMethods()[0];
				return (handlerType, method);
			});

			var handler = _serviceProvider.GetService(handlerInfo.Interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建处理器失败");
			}

			if (handlerInfo.Method.Invoke(handler, new object[] {command, cancellationToken}) is Task task)
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

			var type = command.GetType();
			var handlerInfo = Cache.GetOrAdd(type, x =>
			{
				var handlerType = typeof(ICommandHandler<,>).MakeGenericType(x, typeof(TResponse));
				var method = handlerType.GetMethods()[0];
				return (handlerType, method);
			});

			var handler = _serviceProvider.GetService(handlerInfo.Interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建处理器失败");
			}

			if (handlerInfo.Method.Invoke(handler, new object[] {command, cancellationToken}) is Task<TResponse> task)
			{
				return await task;
			}
			else
			{
				return default;
			}
		}
	}
}