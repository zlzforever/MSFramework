using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 定义无返回值的请求处理器。
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
public interface IRequestHandler<in TRequest> : IScopeDependency where TRequest : Request
{
    /// <summary>
    /// 处理请求。
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// 定义带返回值的请求处理器。
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IScopeDependency where TRequest : Request<TResponse>
{
    /// <summary>
    /// 处理请求并返回响应。
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
