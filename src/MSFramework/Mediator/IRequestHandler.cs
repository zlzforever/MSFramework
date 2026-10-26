using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 定义无返回值的请求处理器。
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <remarks>
/// <b>不做逆变（in）标注</b>：Mediator 按"精确请求类型"闭泛型查找 Handler
/// （<c>GetService(IRequestHandler&lt;ExactRequestType&gt;)</c>，DI 不存在变体匹配），
/// 为请求基类注册的 Handler 永远不会被命中——标注 <c>in</c> 会让这一用法看起来合法，
/// 运行期才报 "Handler not registered"。Handler 必须与请求类型一一对应
/// </remarks>
public interface IRequestHandler<TRequest> : IScopeDependency where TRequest : Request
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
/// <remarks>同 <see cref="IRequestHandler{TRequest}"/>：仅支持精确请求类型匹配，不做逆变标注</remarks>
public interface IRequestHandler<TRequest, TResponse> : IScopeDependency where TRequest : Request<TResponse>
{
    /// <summary>
    /// 处理请求并返回响应。
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
