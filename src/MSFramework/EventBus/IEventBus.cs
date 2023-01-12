using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

public interface IEventBus : IDisposable
{
    /// <summary>
    /// 发布事件
    /// 若使用的是内存型事件总线， Handler 对象是独立的 scope。
    /// 注意要
    /// </summary>
    /// <param name="event"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase;

    void AddInterceptors(InterceptorType type, params Func<IServiceProvider, Task>[] functions);
}
