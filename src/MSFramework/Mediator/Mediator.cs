using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 中介者实现。生命周期: Scoped。
/// 通过 Expression.Compile 将泛型 Handler 的 HandleAsync 方法编译为强类型委托，
/// 避免每次请求的 MethodInfo.Invoke 反射开销和 object[] 分配。
/// </summary>
internal sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, HandlerEntry> RequestCache = new();
    private static readonly ConcurrentDictionary<Type, HandlerEntry> RequestResponseCache = new();

    private sealed record HandlerEntry(Type InterfaceType, Func<object, object, CancellationToken, Task> InvokeAsync);

    /// <summary>
    /// 请求无响应模型 — 只有一个 Handler 响应
    /// </summary>
    /// <param name="request">待发送的请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="ArgumentNullException">request 为 null 时抛出——静默返回成功会把
    /// 调用方"忘记构造请求对象"的 bug 掩盖成"业务成功但什么都没发生"的静默失败</exception>
    public Task SendAsync(Request request, CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));

        var entry = RequestCache.GetOrAdd(request.GetType(),
            static t => CreateEntry(typeof(IRequestHandler<>), t));

        var handler = serviceProvider.GetService(entry.InterfaceType);
        if (handler is null)
        {
            throw new MicroserviceFrameworkException(
                $"Handler not registered: {entry.InterfaceType.FullName}");
        }

        return entry.InvokeAsync(handler, request, cancellationToken);
    }

    /// <summary>
    /// 请求/响应模型 — 只有一个 Handler 响应
    /// </summary>
    /// <param name="request">待发送的请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <returns>表示异步操作的任务，结果为 Handler 返回的响应</returns>
    /// <exception cref="ArgumentNullException">request 为 null 时抛出（与无响应重载一致，拒绝静默成功）</exception>
    public Task<TResponse> SendAsync<TResponse>(Request<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));

        var entry = RequestResponseCache.GetOrAdd(request.GetType(),
            static (t, responseType) => CreateEntry(typeof(IRequestHandler<,>), t, responseType),
            typeof(TResponse));

        var handler = serviceProvider.GetService(entry.InterfaceType);
        if (handler is null)
        {
            throw new MicroserviceFrameworkException(
                $"Handler not registered: {entry.InterfaceType.FullName}");
        }

        var task = entry.InvokeAsync(handler, request, cancellationToken);

        // Expression.Convert 上溯到 Task，但运行时对象仍是 Task<TResponse>
        return (Task<TResponse>)task;
    }

    /// <summary>
    /// 发布模型 — 所有注册的 Handler 都会响应。
    /// <para>
    /// 执行语义：<b>按注册顺序逐个执行，失败即中断</b>——第一个抛出异常的 Handler 终止后续
    /// Handler 并向调用方传播。本框架中 Publish 仅用于领域事件派发场景（DbContextBase 在数据库
    /// 提交前派发、与业务同事务）：任一 Handler 失败即放弃本次保存（整体不提交），
    /// "失败即中断"正是期望语义。未注册任何 Handler 时静默完成（事件可选订阅）。
    /// Handler 之间仍应相互独立、幂等，不应有顺序或依赖关系。
    /// </para>
    /// </summary>
    /// <param name="request">待发布的请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="ArgumentNullException">request 为 null 时抛出（与 SendAsync 语义一致）</exception>
    public async Task PublishAsync(Request request, CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));

        var entry = RequestCache.GetOrAdd(request.GetType(),
            static t => CreateEntry(typeof(IRequestHandler<>), t));

        // 注意：发布模式下可能没有 Handler，这是合法的（事件可选订阅）
        var handlers = serviceProvider.GetServices(entry.InterfaceType);
        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            await entry.InvokeAsync(handler, request, cancellationToken);
        }
    }

    private static HandlerEntry CreateEntry(Type genericHandlerType, Type requestType,
        params Type[] extraTypeArgs)
    {
        var typeArgs = new Type[1 + extraTypeArgs.Length];
        typeArgs[0] = requestType;
        Array.Copy(extraTypeArgs, 0, typeArgs, 1, extraTypeArgs.Length);

        var handlerType = genericHandlerType.MakeGenericType(typeArgs);
        var method = handlerType.GetMethod("HandleAsync", BindingFlags.Public | BindingFlags.Instance)!;
        var func = HandlerDelegateBuilder.Build(handlerType, method, requestType);

        return new HandlerEntry(handlerType, func);
    }
}
