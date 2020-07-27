using System;
using System.Threading;
using System.Threading.Tasks;
using MSFramework.Extensions;

namespace MSFramework.Application
{
	public interface IRequestExecutor
	{
		void Register(Type requestType, Type handlerType);

		Task ExecuteAsync(IRequest request, CancellationToken cancellationToken = default);

		Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
	}
}