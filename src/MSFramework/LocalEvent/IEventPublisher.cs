using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 本地事件发布器——进程内、尽力而为（best-effort）的事件通道。
/// <para>
/// 可靠性语义（使用前必读）：
/// </para>
/// <list type="bullet">
/// <item><description><b>无重试、无死信</b>：Handler 抛出的异常由后台服务捕获并仅记录日志，
/// 事件随即丢弃，不会重投；需要重试/补偿的业务必须在 Handler 内自行实现
/// （try-catch、退避、状态表等），并保证 Handler 幂等；</description></item>
/// <item><description><b>停机不保证排空</b>：Host 停止时管道中未消费的事件直接丢失，
/// 发布成功 ≠ 一定被处理；</description></item>
/// <item><description><b>发布端背压</b>：管道容量 2000，写满时最多等待
/// <see cref="LocalEventOptions.PublishTimeout"/>（默认 30 秒），可在单次发布时覆盖，超时拒绝本次发布；</description></item>
/// <item><description>事件的消费<b>独立于请求处理线程与请求 Scope</b>：发布后由后台服务
/// （BackgroundService）按序消费；<b>同一事件</b>的多个 Handler 在同一后台循环内按注册顺序
/// 逐个执行（非并行、各自独立 Scope）——依赖"多个 Handler 并发互不阻塞"假设的处理逻辑
/// 需自行拆分事件；</description></item>
/// <item><description>适用场景：不影响主业务的旁路处理（发送邮件/短信、统计上报等）；
/// <b>更高可靠性要求</b>（不可丢失、需要重试/死信、跨服务分发）请使用集成事件（消息队列：
/// CAP 本地消息表 Outbox / Dapr Pub/Sub / RabbitMQ 等）。</description></item>
/// </list>
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// 发布事件到进程内管道。"发布成功"仅表示事件已入队，不保证会被处理（见接口级可靠性语义）。
    /// Handler 在独立 scope 中执行；若 Handler 有数据库操作，需注意其工作单元独立于发布方——
    /// 发布方可能要先提交自身工作单元，否则可能出现两边数据不一致
    /// </summary>
    /// <param name="event">待发布的事件，不允许为 null</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <returns>表示异步操作的任务（入队完成后完成，不代表处理完成）</returns>
    /// <exception cref="System.ArgumentNullException">事件为空时抛出。</exception>
    /// <exception cref="System.TimeoutException">等待队列空位超过配置时长时抛出。</exception>
    /// <exception cref="System.OperationCanceledException">调用方取消发布时抛出。</exception>
    /// <exception cref="System.Threading.Channels.ChannelClosedException">宿主停止后通道已关闭时抛出。</exception>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : EventBase;

    /// <summary>
    /// 使用本次指定的超时发布事件，覆盖全局配置但不修改配置值；超时仅限制等待队列空位，
    /// 入队成功不代表处理成功，也不限制处理器执行时间。
    /// </summary>
    /// <typeparam name="TEvent">继承 EventBase 的事件类型。</typeparam>
    /// <param name="event">待发布事件，不允许为空。</param>
    /// <param name="timeout">本次入队等待上限；为 null 时使用全局配置（默认 30 秒）。非空时必须大于零且不超过 4294967294 毫秒，不支持无限等待。</param>
    /// <param name="cancellationToken">调用方取消令牌，取消时保留取消异常语义。</param>
    /// <returns>事件入队完成时完成的任务，不代表处理完成。</returns>
    /// <exception cref="ArgumentNullException">事件为空。</exception>
    /// <exception cref="ArgumentOutOfRangeException">超时不在支持范围内。</exception>
    /// <exception cref="TimeoutException">在指定时间内未获得队列空位。</exception>
    /// <exception cref="OperationCanceledException">调用方取消发布。</exception>
    /// <exception cref="System.Threading.Channels.ChannelClosedException">宿主停止后通道已关闭。</exception>
    Task PublishAsync<TEvent>(TEvent @event, TimeSpan? timeout, CancellationToken cancellationToken = default)
        where TEvent : EventBase;
}
