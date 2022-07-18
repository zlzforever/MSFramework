using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

public interface IRequestHandler<in TRequest> : IScopeDependency where TRequest : IRequest
{
    Task HandleAsync(TRequest query, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest, TResponse> : IScopeDependency where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest query, CancellationToken cancellationToken = default);
}
