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
    ///
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
