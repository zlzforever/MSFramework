using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 中介者
/// </summary>
public interface IMediator : IScopeDependency
{
    /// <summary>
    /// 请求模型
    /// 只有最后一个注册的 Handler 会响应请求
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync(Request request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 请求/响应模型
    /// 只有最后一个注册的 Handler 会响应请求
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    Task<TResponse> SendAsync<TResponse>(Request<TResponse> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布模型： 所有注册的 Handler 都会响应
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(Request message, CancellationToken cancellationToken = default);
}
