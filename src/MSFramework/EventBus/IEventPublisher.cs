namespace MicroserviceFramework.EventBus;

/// <summary>
/// 事件发布器
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// 发布事件
    /// Handler 对象是独立的 scope
    /// 事件发出后，会在独立的线程中执行，若有数据库操作，则需要注意可能要自己先提交 UOW，不然可能会导到两边数据不一致的情况
    /// </summary>
    /// <param name="event"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    void Publish<TEvent>(TEvent @event) where TEvent : EventBase;

    void Publish(string name, object @event);
}
