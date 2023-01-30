using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

/// <summary>
/// 仅用于内存型本地事件, 不建议生产大规模使用
/// 分布式事件， 统一使用 ICapPublisher
/// </summary>
[Obsolete]
public interface IEventBus : IDisposable
{
    /// <summary>
    /// 发布事件
    /// Handler 对象是独立的 scope
    /// 注意要
    /// </summary>
    /// <param name="event"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase;

    void AddInterceptors(InterceptorType type, params Func<IServiceProvider, Task>[] functions);
}
