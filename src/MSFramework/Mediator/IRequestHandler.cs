using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

/// <summary>
///
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface IRequestHandler<in TRequest> : IScopeDependency where TRequest : Request
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
///
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IScopeDependency where TRequest : Request<TResponse>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
