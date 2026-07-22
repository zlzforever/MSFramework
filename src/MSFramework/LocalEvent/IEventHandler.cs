using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 事件处理器
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : EventBase
{
    /// <summary>
    /// 处理事件。
    /// </summary>
    /// <param name="event">事件对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
