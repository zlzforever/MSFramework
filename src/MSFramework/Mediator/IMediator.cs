using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Mediator
{
	public interface IMediator
	{
		Task SendAsync(IRequest request, CancellationToken cancellationToken = default);

		Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> query, CancellationToken cancellationToken = default);
	}
}