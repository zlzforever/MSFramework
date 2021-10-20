using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Mediator
{
	internal class Mediator : IMediator
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)> Cache;

		static Mediator()
		{
			Cache = new ConcurrentDictionary<Type, (Type, MethodInfo)>();
		}

		public Mediator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				return;
			}

			var queryType = request.GetType();

			var (@interface, method) = Cache.GetOrAdd(queryType, type => Create(typeof(IRequestHandler<>), type));

			var handler = _serviceProvider.GetService(@interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建查询处理器失败");
			}

			if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task task)
			{
				await task;
			}
		}

		public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
			CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				return default;
			}

			var queryType = request.GetType();
			var (@interface, method) =
				Cache.GetOrAdd(queryType, type => Create(typeof(IRequestHandler<,>), type, typeof(TResponse)));
			var handler = _serviceProvider.GetService(@interface);
			if (handler == null)
			{
				throw new MicroserviceFrameworkException("创建查询处理器失败");
			}

			if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task<TResponse> task)
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