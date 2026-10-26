using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 本地事件处理器契约。
/// <para>
/// 可靠性语义（实现前必读，与 <see cref="IEventPublisher"/> 一致）：
/// </para>
/// <list type="bullet">
/// <item><description><see cref="HandleAsync"/> 抛出的异常会被后台服务捕获并仅记录日志，
/// 事件<b>不会重试、无死信</b>——需要重试/补偿的业务必须在 Handler 内自行实现，
/// 并保证 Handler 幂等；</description></item>
/// <item><description>Handler 在独立 scope 中执行，可注入 Scoped 服务（含 DbContext），
/// 但其工作单元与发布方相互独立；</description></item>
/// <item><description>Host 停止时未消费的事件直接丢失，不要在 Handler 中实现必须完成的关键副作用。</description></item>
/// </list>
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
/// <remarks>
/// <b>不做逆变（in）标注</b>：本地事件按"精确事件类型"派发——
/// <see cref="EventDescriptorStore"/> 注册/查找 Handler 均以精确事件类型为键
/// （HandleAsync 必须是 <c>HandleAsync(精确事件类型, CancellationToken)</c>），
/// 为事件基类注册的 Handler 永远不会被派发命中；标注 <c>in</c> 会让这一用法看起来合法。
/// Handler 必须与事件类型一一对应
/// </remarks>
public interface IEventHandler<TEvent>
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
