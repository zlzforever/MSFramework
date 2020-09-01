using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Application.CQRS.Query
{
	public interface IQueryHandler<in TQuery> where TQuery : IQuery
	{
		Task HandleAsync(TQuery query, CancellationToken cancellationToken = default);
	}

	public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
	{
		Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
	}
}