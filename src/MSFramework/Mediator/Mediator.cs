using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 生命周期: Scoped
/// </summary>
internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    // TODO: 可以考虑启动时全加载
    private static readonly Lazy<ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)>> HandlerCache = new();

    /// <summary>
    /// 请求无响应模型
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public Task SendAsync(Request request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return Task.CompletedTask;
        }

        var requestType = request.GetType();
        var (@interface, method) =
            HandlerCache.Value.GetOrAdd(requestType, type => CreateHandlerMeta(typeof(IRequestHandler<>), type));

        var handler = serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException(
                $"创建处理器 IRequestHandler<{requestType.FullName}> 失败");
        }

        return HandleAsync(request, handler, method, cancellationToken);
    }

    /// <summary>
    /// 请求响应模型
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public Task<TResponse> SendAsync<TResponse>(Request<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return null;
        }

        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var (@interface, method) =
            HandlerCache.Value.GetOrAdd(requestType,
                type => CreateHandlerMeta(typeof(IRequestHandler<,>), type, responseType));
        var handler = serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException(
                $"创建处理器 IRequestHandler<{requestType.FullName}, {responseType.FullName}> 失败");
        }

        var result = method.Invoke(handler, new object[] { request, cancellationToken });
        if (result is Task<TResponse> task)
        {
            return task;
        }

        throw new MicroserviceFrameworkException(
            $"处理器 IRequestHandler<{requestType.FullName}, {responseType.FullName}> 返回类型不正确");
    }

    /// <summary>
    /// 多个响应
    /// 注意：Handler 之间应该是独立的，不应该有依赖、顺序关系，并且必须所有 Handler 都能执行成功
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    public async Task PublishAsync(Request request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return;
        }

        var (@interface, method) =
            HandlerCache.Value.GetOrAdd(request.GetType(), type => CreateHandlerMeta(typeof(IRequestHandler<>), type));

        var handlers = serviceProvider.GetServices(@interface);
        foreach (var handler in handlers)
        {
            if (handler == null)
            {
                continue;
            }

            await HandleAsync(request, handler, method, cancellationToken);
        }
    }

    private Task HandleAsync(Request request, object handler, MethodInfo method,
        CancellationToken cancellationToken)
    {
        if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task task)
        {
            return task;
        }

        return Task.CompletedTask;
    }

    private static (Type HandlerType, MethodInfo MethodInfo) CreateHandlerMeta(Type type, params Type[] typeArguments)
    {
        var handlerType = type.MakeGenericType(typeArguments);
        var method = handlerType.GetMethods()[0];
        return (handlerType, method);
    }
}
