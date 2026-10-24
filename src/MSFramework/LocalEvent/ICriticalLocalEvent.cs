namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 标记不能接受进程内 best-effort 交付语义的关键事件。
/// 此类事件必须通过 outbox、Dapr 或其他持久消息系统发布。
/// </summary>
public interface ICriticalLocalEvent;
