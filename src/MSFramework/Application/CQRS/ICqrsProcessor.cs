using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Application.CQRS
{
	public interface ICqrsProcessor
	{
		Task QueryAsync(IQuery request, CancellationToken cancellationToken = default);

		Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default);

		Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default);

		Task<TResponse> ExecuteAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
	}
}