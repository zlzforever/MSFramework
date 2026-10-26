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
    /// 请求模型：只执行一个 Handler。
    /// </summary>
    /// <remarks>
    /// 设计决策（勿在后续 review 中重复提出）：一个请求只允许注册一个 Handler，
    /// <c>GetService</c> 命中最后注册者为预期行为而非缺陷——request/response 模型必须返回
    /// 确定的响应，多 Handler 时响应无法合并。需要多个处理器协同的场景请使用
    /// <see cref="PublishAsync"/>（全部 Handler 都执行），或按职责拆分请求。
    /// </remarks>
    /// <param name="request">请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>无响应的请求直接返回</returns>
    Task SendAsync(Request request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 请求/响应模型：只执行一个 Handler 并返回其响应。
    /// </summary>
    /// <remarks>
    /// 设计决策（勿在后续 review 中重复提出）：一个请求只允许注册一个 Handler，
    /// <c>GetService</c> 命中最后注册者为预期行为而非缺陷——request/response 模型必须返回
    /// 确定的响应，多 Handler 时响应无法合并。需要多个处理器协同的场景请使用
    /// <see cref="PublishAsync"/>（全部 Handler 都执行），或按职责拆分请求。
    /// </remarks>
    /// <param name="request">请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <returns>唯一 Handler 的响应</returns>
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
