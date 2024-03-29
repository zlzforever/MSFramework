using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

public interface IRequestHandler<in TRequest> : IScopeDependency where TRequest : Request
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest, TResponse> : IScopeDependency where TRequest : Request<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
