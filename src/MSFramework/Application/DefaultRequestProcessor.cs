using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Application
{
	public class DefaultRequestProcessor : IRequestProcessor
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly RequestHandlerTypeCache _cache;

		public DefaultRequestProcessor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_cache = _serviceProvider.GetRequiredService<RequestHandlerTypeCache>();
		}

		public async Task ProcessAsync(IRequest request, CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestType = request.GetType();
			if (_cache.TryGetValue(requestType, out (Type Type, MethodInfo Method) tuple))
			{
				var handler = _serviceProvider.GetRequiredService(tuple.Type);
				await (Task) tuple.Method.Invoke(handler, new object[] {request, cancellationToken});
			}
			else
			{
				throw new ApplicationException("找不到命令处理器");
			}
		}

		public async Task<TResult> ProcessAsync<TResult>(IRequest<TResult> request,
			CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestType = request.GetType();
			if (_cache.TryGetValue(requestType, out (Type Type, MethodInfo Method) tuple))
			{
				var handler = _serviceProvider.GetRequiredService(tuple.Type);
				return await (Task<TResult>) tuple.Method.Invoke(handler, new object[] {request, cancellationToken});
			}
			else
			{
				throw new ApplicationException("找不到命令处理器");
			}
		}
	}
}