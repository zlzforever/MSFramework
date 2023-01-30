using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.EventBus;

[Obsolete]
internal class InProcessEventBus : IEventBus
{
    private readonly List<Func<IServiceProvider, Task>> _beforeFunctions;
    private readonly List<Func<IServiceProvider, Task>> _afterFunctions;
    private readonly IServiceProvider _serviceProvider;

    public InProcessEventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _beforeFunctions = new();
        _afterFunctions = new();
    }

    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
    {
        Check.NotNull(@event, nameof(@event));

        Task.Factory.StartNew(async (e) =>
        {
            // 主动延迟 500 豪秒， 模拟消息队列延迟， 提供时间本进程提交数据
            await Task.Delay(500);

            using var scope = _serviceProvider.CreateScope();
            var eventExecutor = scope.ServiceProvider.GetRequiredService<IEventProcessor>();
            foreach (var function in _beforeFunctions)
            {
                await function(scope.ServiceProvider);
            }

            await eventExecutor.ExecuteAsync(@event.GetType().GetEventName(),
                Defaults.JsonHelper.Serialize(@event));

            foreach (var function in _afterFunctions)
            {
                await function(scope.ServiceProvider);
            }
        }, @event).ConfigureAwait(false);

        return Task.CompletedTask;
    }

    public void AddInterceptors(InterceptorType type, params Func<IServiceProvider, Task>[] functions)
    {
        foreach (var func in functions)
        {
            if (type == InterceptorType.After)
            {
                _afterFunctions.Add(func);
            }
            else
            {
                _beforeFunctions.Add(func);
            }
        }
    }

    public void Dispose()
    {
    }
}
