using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.Application.CQRS
{
	public class CqrsProcessor : ICqrsProcessor
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly Dictionary<Type, (Type Interface, MethodInfo Method)> Cache;

		static CqrsProcessor()
		{
			Cache = new Dictionary<Type, (Type, MethodInfo)>();
		}

		public CqrsProcessor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public static void Register(Type eventType, (Type Interface, MethodInfo Method) cacheItem)
		{
			lock (Cache)
			{
				if (!Cache.ContainsKey(eventType))
				{
					Cache.Add(eventType, cacheItem);
				}
			}
		}

		public async Task QueryAsync(IQuery request, CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestType = request.GetType();

			var tuple = Cache.GetOrDefault(requestType);

			if (tuple == default)
			{
				throw new MicroserviceFrameworkException("获取处理器缓存失败");
			}

			var handler = _serviceProvider.GetService(tuple.Interface);

			if (tuple.Method.Invoke(handler, new object[] {request, cancellationToken}) is Task task)
			{
				await task;
			}
		}

		public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> request,
			CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestType = request.GetType();

			var tuple = Cache.GetOrDefault(requestType);

			if (tuple == default)
			{
				throw new MicroserviceFrameworkException("获取处理器缓存失败");
			}

			var handler = _serviceProvider.GetService(tuple.Interface);

			if (tuple.Method.Invoke(handler, new object[] {request, cancellationToken}) is Task<TResponse> task)
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

			var commandType = command.GetType();

			var tuple = Cache.GetOrDefault(commandType);

			if (tuple == default)
			{
				throw new MicroserviceFrameworkException("获取处理器缓存失败");
			}

			var handler = _serviceProvider.GetService(tuple.Interface);

			if (tuple.Method.Invoke(handler, new object[] {command, cancellationToken}) is Task task)
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

			var tuple = Cache.GetOrDefault(commandType);

			if (tuple == default)
			{
				throw new MicroserviceFrameworkException("获取处理器缓存失败");
			}

			var handler = _serviceProvider.GetService(tuple.Interface);

			if (tuple.Method.Invoke(handler, new object[] {command, cancellationToken}) is Task<TResponse> task)
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