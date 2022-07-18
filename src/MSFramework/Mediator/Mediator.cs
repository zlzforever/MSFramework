using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Mediator;

internal class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediatorTypeMapper _mediatorTypeMapper;

    public Mediator(IServiceProvider serviceProvider, IMediatorTypeMapper mediatorTypeMapper)
    {
        _serviceProvider = serviceProvider;
        _mediatorTypeMapper = mediatorTypeMapper;
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

        var queryType = request.GetType();

        var (@interface, method) =
            _mediatorTypeMapper.Get(queryType, type => Create(typeof(IRequestHandler<>), type));

        var handler = _serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException("创建查询处理器失败");
        }

        if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task task)
        {
            await task;
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

        var queryType = request.GetType();
        var (@interface, method) =
            _mediatorTypeMapper.Get(queryType, type => Create(typeof(IRequestHandler<,>), type, typeof(TResponse)));
        var handler = _serviceProvider.GetService(@interface);
        if (handler == null)
        {
            throw new MicroserviceFrameworkException("创建查询处理器失败");
        }

        if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task<TResponse> task)
        {
            return await task;
        }

        return default;
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
            _mediatorTypeMapper.Get(messageType, type => Create(typeof(IRequestHandler<>), type));

        var handlers = _serviceProvider.GetServices(@interface).Where(x => x != null);

        foreach (var handler in handlers)
        {
            if (method.Invoke(handler, new object[] { request, cancellationToken }) is Task task)
            {
                await task;
            }
        }
    }

    private static (Type HandlerType, MethodInfo MethodInfo) Create(Type type, params Type[] typeArguments)
    {
        var handlerType = type.MakeGenericType(typeArguments);
        var method = handlerType.GetMethods()[0];
        return (handlerType, method);
    }
}
