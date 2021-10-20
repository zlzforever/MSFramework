using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;

namespace MicroserviceFramework.Mediator
{
	public interface IRequestHandler<in TQuery> : IScopeDependency where TQuery : IRequest
	{
		Task HandleAsync(TQuery query, CancellationToken cancellationToken = default);
	}

	public interface IRequestHandler<in TQuery, TResponse> : IScopeDependency where TQuery : IRequest<TResponse>
	{
		Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
	}
}