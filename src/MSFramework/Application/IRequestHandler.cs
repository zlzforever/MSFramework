using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Application
{
	public interface IRequestHandler
	{
	}

	public interface IRequestHandler<in TRequest> : IRequestHandler where TRequest : IRequest
	{
		Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
	}

	public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler where TRequest : IRequest<TResponse>
	{
		Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
	}
}