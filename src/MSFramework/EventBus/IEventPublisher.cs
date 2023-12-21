namespace MicroserviceFramework.EventBus;

/// <summary>
/// 事件发布器
/// 警告：使用本地事件是独立的 Task + Scope
/// 警告：本地事件尽量只使用在不影响业务的情况下使用，如：发送邮件，短信等，即使失败也不会有太大影响
/// 若是集成事件，如：订单创建事件，支付成功事件等，建议使用分布式事件/SAGA
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
}
