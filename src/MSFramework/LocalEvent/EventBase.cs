namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 事件基类。
/// 本地事件为进程内尽力而为语义（无重试/死信、停机不保证排空，见 <see cref="IEventPublisher"/>）；
/// 更高可靠性要求请使用集成事件（消息队列）。
/// </summary>
public abstract record EventBase
{
    // public static readonly Type EventBaseType = typeof(EventBase);
}
