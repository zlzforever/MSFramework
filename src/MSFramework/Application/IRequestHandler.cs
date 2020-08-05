using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Application
{
	public interface IRequestHandler<in TRequest> where TRequest : IRequest
	{
		Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
	}

	public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
	{
		Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
	}
}