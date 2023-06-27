using System;
using System.Collections.Concurrent;
using System.Linq;
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
    private static readonly ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)> HandlerCache;

    static Mediator()
    {
        HandlerCache = new ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)>();
    }

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 单个响应
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return;
        }

        var requestType = request.GetType();

        var (@interface, method) =
            HandlerCache.GetOrAdd(requestType, type => CreateHandlerMeta(typeof(IRequestHandler<>), type));

        var handler = _serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException("创建查询处理器失败");
        }

        var traceId = ObjectId.GenerateNewId().ToString();

        try
        {
            _logger.LogDebug("{TraceId}, {HandlerType} 开始处理请求 {Request}", traceId, handler.GetType().FullName,
                Defaults.JsonHelper.Serialize(request));

            if (method.Invoke(handler, new object[] { request, cancellationToken }) is not Task task)
            {
                return;
            }

            await task;

            _logger.LogDebug("{TraceId}, {HandlerType} 处理成功", traceId, handler.GetType().FullName);
        }
        catch (Exception e)
        {
            _logger.LogError("{TraceId}, {HandlerType} 处理失败", traceId, handler.GetType().FullName);

            if (e.InnerException != null)
            {
                throw e.InnerException;
            }

            throw;
        }
    }

    /// <summary>
    /// 单个响应
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return default;
        }

        var requestType = request.GetType();
        var (@interface, method) =
            HandlerCache.GetOrAdd(requestType,
                type => CreateHandlerMeta(typeof(IRequestHandler<,>), type, typeof(TResponse)));
        var handler = _serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException("创建查询处理器失败");
        }

        var traceId = ObjectId.GenerateNewId().ToString();

        try
        {
            _logger.LogDebug("{TraceId}, {HandlerType} 开始处理请求 {Request}", traceId, handler.GetType().FullName,
                Defaults.JsonHelper.Serialize(request));

            if (method.Invoke(handler, new object[] { request, cancellationToken }) is not Task<TResponse> task)
            {
                _logger.LogWarning("{TraceId}, {HandlerType} 处理器没有返回值", traceId, handler.GetType().FullName);
                return default;
            }

            var result = await task;
            _logger.LogDebug("{TraceId}, {HandlerType} 处理成功", traceId, handler.GetType().FullName);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("{TraceId}, {HandlerType} 处理失败", traceId, handler.GetType().FullName);

            if (e.InnerException != null)
            {
                throw e.InnerException;
            }

            throw;
        }
    }

    /// <summary>
    /// 多个响应
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    public async Task PublishAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return;
        }

        var messageType = request.GetType();

        var (@interface, method) =
            HandlerCache.GetOrAdd(messageType, type => CreateHandlerMeta(typeof(IRequestHandler<>), type));

        var handlers = _serviceProvider.GetServices(@interface).Where(x => x != null);

        var traceId = ObjectId.GenerateNewId().ToString();

        foreach (var handler in handlers)
        {
            try
            {
                _logger.LogDebug("{TraceId}, {HandlerType} 开始处理请求 {Request}", traceId, handler.GetType().FullName,
                    Defaults.JsonHelper.Serialize(request));

                if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task task)
                {
                    await task;
                }

                _logger.LogDebug("{TraceId}, {HandlerType} 处理成功", traceId, handler.GetType().FullName);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logger.LogError("{TraceId}, {HandlerType} 处理失败", traceId, handler.GetType().FullName);
                    throw e.InnerException;
                }

                _logger.LogError("{TraceId}, {HandlerType} 处理失败", traceId, handler.GetType().FullName);
                throw;
            }
        }
    }

    private static (Type HandlerType, MethodInfo MethodInfo) CreateHandlerMeta(Type type, params Type[] typeArguments)
    {
        var handlerType = type.MakeGenericType(typeArguments);
        var method = handlerType.GetMethods()[0];
        return (handlerType, method);
    }
}
