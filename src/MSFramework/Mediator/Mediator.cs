using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    public Task SendAsync(Request request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Task.CompletedTask;
        }

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
    public Task<TResponse> SendAsync<TResponse>(Request<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Task.FromResult<TResponse>(default);
        }

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
    /// Handler 之间应相互独立，不应有顺序或依赖关系。
    /// </summary>
    public async Task PublishAsync(Request request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return;
        }

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
        var func = BuildDelegate(handlerType, method, requestType);

        return new HandlerEntry(handlerType, func);
    }

    private static Func<object, object, CancellationToken, Task> BuildDelegate(
        Type handlerType, MethodInfo method, Type requestType)
    {
        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        Expression body = Expression.Call(
            Expression.Convert(handlerParam, handlerType),
            method,
            Expression.Convert(requestParam, requestType),
            ctParam);

        // 返回值若是 Task<TResponse> 则上溯到 Task，以匹配委托签名
        if (method.ReturnType != typeof(Task))
        {
            body = Expression.Convert(body, typeof(Task));
        }

        return Expression.Lambda<Func<object, object, CancellationToken, Task>>(
            body, handlerParam, requestParam, ctParam).Compile();
    }
}
