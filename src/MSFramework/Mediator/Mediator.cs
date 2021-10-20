using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator
{
	internal class Mediator : IMediator
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IMediatorTypeMapper _mediatorTypeMapper;

		public Mediator(IServiceProvider serviceProvider, IMediatorTypeMapper mediatorTypeMapper)
		{
			_serviceProvider = serviceProvider;
			_mediatorTypeMapper = mediatorTypeMapper;
		}

		/// <summary>
		/// 只能有一个响应
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <exception cref="MicroserviceFrameworkException"></exception>
		public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				return;
			}

			var queryType = request.GetType();

			var (@interface, method) =
				_mediatorTypeMapper.Get(queryType, type => Create(typeof(IRequestHandler<>), type));

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
				_mediatorTypeMapper.Get(queryType, type => Create(typeof(IRequestHandler<,>), type, typeof(TResponse)));
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


		public async Task PublishAsync(IMessage message, CancellationToken cancellationToken = default)
		{
			if (message == null)
			{
				return;
			}

			var messageType = message.GetType();

			var (@interface, method) =
				_mediatorTypeMapper.Get(messageType, type => Create(typeof(IMessageHandler<>), type));

			var handlers = _serviceProvider.GetServices(@interface).Where(x => x != null);

			if (messageType.Name == "Event4")
			{
			}

			foreach (var handler in handlers)
			{
				if (method.Invoke(handler, new object[] { message }) is Task task)
				{
					await task;
				}
			}
		}

		private static (Type HandlerType, MethodInfo MethodInfo) Create(Type type, params Type[] typeArguments)
		{
			var handlerType = type.MakeGenericType(typeArguments);
			var method = handlerType.GetMethods()[0];
			return (handlerType, method);
		}
	}
}