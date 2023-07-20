using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

/// <summary>
/// 仅用于内存型本地事件, 不建议生产大规模使用
/// 分布式事件， 统一使用 ICapPublisher/Dapr event
/// </summary>
public interface IEventBus : IDisposable
{
    /// <summary>
    /// 发布事件
    /// Handler 对象是独立的 scope
    /// 事件发出后，会在独立的线程中执行，若有数据库操作，则需要注意可能要自己先提交 UOW，不然可能会导到两边数据不一致的情况
    /// </summary>
    /// <param name="event"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase;

    void AddInterceptors(InterceptorType type, params Func<IServiceProvider, Task>[] functions);
}
