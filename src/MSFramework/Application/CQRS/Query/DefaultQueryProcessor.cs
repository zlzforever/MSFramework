using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Application.CQRS.Query
{
	public class DefaultQueryProcessor : IQueryProcessor
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly HandlerTypeCache _cache;

		public DefaultQueryProcessor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_cache = _serviceProvider.GetRequiredService<HandlerTypeCache>();
		}

		public async Task QueryAsync(IQuery request, CancellationToken cancellationToken = default)
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
				throw new ApplicationException("找不到查询处理器");
			}
		}

		public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> request,
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
				throw new ApplicationException("找不到查询处理器");
			}
		}
	}
}