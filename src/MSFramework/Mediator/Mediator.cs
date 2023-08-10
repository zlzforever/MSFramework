using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 生命周期: Scoped
/// </summary>
internal class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;
    private static readonly Lazy<ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)>> HandlerCache = new();

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 请求无响应模型
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public async Task SendAsync(Request request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return;
        }

        var requestType = request.GetType();
        var (@interface, method) =
            HandlerCache.Value.GetOrAdd(requestType, type => CreateHandlerMeta(typeof(IRequestHandler<>), type));

        var handler = _serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException(
                $"创建处理器 IRequestHandler<{requestType.FullName}> 失败");
        }

        var traceId = ObjectId.GenerateNewId().ToString();
        await HandleAsync(traceId, request, handler, method, cancellationToken);
    }

    /// <summary>
    /// 请求响应模型
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public async Task<TResponse> SendAsync<TResponse>(Request<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return default;
        }

        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var (@interface, method) =
            HandlerCache.Value.GetOrAdd(requestType,
                type => CreateHandlerMeta(typeof(IRequestHandler<,>), type, responseType));
        var handler = _serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException(
                $"创建处理器 IRequestHandler<{requestType.FullName}, {responseType.FullName}> 失败");
        }

        var traceId = ObjectId.GenerateNewId().ToString();

        try
        {
            _logger.LogDebug("{TraceId}, 处理器 {HandlerType} 开始处理 {Request}", traceId, handler.GetType().FullName,
                Defaults.JsonSerializer.Serialize(request));

            TResponse result = default;
            var invokeResult = method.Invoke(handler, new object[] { request, cancellationToken });
            if (invokeResult is Task<TResponse> task)
            {
                result = await task;
            }

            _logger.LogDebug("{TraceId}, 处理器 {HandlerType} 处理成功", traceId, handler.GetType().FullName);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("{TraceId}, 处理器 {HandlerType} 处理失败", traceId, handler.GetType().FullName);
            throw e.InnerException ?? e;
        }
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

        var handlers = _serviceProvider.GetServices(@interface);

        var traceId = ObjectId.GenerateNewId().ToString();

        foreach (var handler in handlers)
        {
            if (handler == null)
            {
                continue;
            }

            await HandleAsync(traceId, request, handler, method, cancellationToken);
        }
    }

    private async Task HandleAsync(string traceId, Request request, object handler, MethodInfo method,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("{TraceId}, 处理器 {HandlerType} 开始处理 {Request}", traceId, handler.GetType().FullName,
                Defaults.JsonSerializer.Serialize(request));

            if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task task)
            {
                await task;
            }

            _logger.LogDebug("{TraceId}, 处理器 {HandlerType} 处理成功", traceId, handler.GetType().FullName);
        }
        catch (Exception e)
        {
            _logger.LogError("{TraceId}, 处理器 {HandlerType} 处理失败", traceId, handler.GetType().FullName);
            throw e.InnerException ?? e;
        }
    }

    private static (Type HandlerType, MethodInfo MethodInfo) CreateHandlerMeta(Type type, params Type[] typeArguments)
    {
        var handlerType = type.MakeGenericType(typeArguments);
        var method = handlerType.GetMethods()[0];
        return (handlerType, method);
    }
}
