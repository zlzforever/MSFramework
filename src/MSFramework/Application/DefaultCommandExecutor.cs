using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Application
{
	public class DefaultRequestExecutor : IRequestExecutor
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly RequestHandlerTypeCache _cache;

		public DefaultRequestExecutor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_cache = _serviceProvider.GetRequiredService<RequestHandlerTypeCache>();
		}

		public void Register(Type requestType, Type handlerType)
		{
			if (requestType == null)
			{
				throw new ArgumentNullException(nameof(requestType));
			}

			if (handlerType == null)
			{
				throw new ArgumentNullException(nameof(handlerType));
			}

			if (!typeof(IRequest).IsAssignableFrom(requestType))
			{
				throw new ArgumentException(
					$"Request {requestType.FullName} should inherit from IRequest or IRequest<>");
			}

			var handleRequestType = handlerType.GetInterface("IRequestHandler`1")?.GenericTypeArguments
				.FirstOrDefault();
			if (handleRequestType != null && handleRequestType == requestType)
			{
				if (_cache.ContainsKey(requestType))
				{
					throw new ArgumentException(
						$"There are more than 1 request handler for request: [{requestType.FullName}]");
				}

				_cache.TryAdd(requestType, (handlerType, handlerType.GetMethod("HandleAsync")));
			}
			else
			{
				throw new ArgumentException($"Type {handlerType} is not a valid request handler");
			}
		}

		public async Task ExecuteAsync(IRequest request, CancellationToken cancellationToken)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var commandType = request.GetType();
			if (_cache.TryGetValue(commandType, out (Type Type, MethodInfo Method) tuple))
			{
				var handler = _serviceProvider.GetRequiredService(tuple.Type);
				await (Task) tuple.Method.Invoke(handler, new object[] {request, cancellationToken});
			}
			else
			{
				throw new ApplicationException("找不到命令处理器");
			}
		}

		public async Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> command,
			CancellationToken cancellationToken)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			var commandType = command.GetType();
			if (_cache.TryGetValue(commandType, out (Type Type, MethodInfo Method) tuple))
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