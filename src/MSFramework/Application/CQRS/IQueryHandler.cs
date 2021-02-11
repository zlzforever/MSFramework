using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;

namespace MicroserviceFramework.Application.CQRS
{
	public interface IQueryHandler<in TQuery> : IScopeDependency where TQuery : IQuery
	{
		Task HandleAsync(TQuery query, CancellationToken cancellationToken = default);
	}

	public interface IQueryHandler<in TQuery, TResponse> : IScopeDependency where TQuery : IQuery<TResponse>
	{
		Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
	}
}