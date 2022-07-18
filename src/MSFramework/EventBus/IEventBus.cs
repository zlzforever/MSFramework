using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

public interface IEventBus : IDisposable
{
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <param name="event"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase;
}
