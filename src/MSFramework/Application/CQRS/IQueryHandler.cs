using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Application.CQRS
{
	public interface IQueryHandler<in TRequest> where TRequest : IQuery
	{
		Task HandleAsync(TRequest query, CancellationToken cancellationToken = default);
	}

	public interface IQueryHandler<in TRequest, TResponse> where TRequest : IQuery<TResponse>
	{
		Task<TResponse> HandleAsync(TRequest query, CancellationToken cancellationToken = default);
	}
}