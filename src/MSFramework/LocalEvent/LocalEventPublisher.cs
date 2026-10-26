using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 将事件与发布者会话快照写入有界通道；依赖配置提供有限的背压等待时间，不保证消费成功。
/// </summary>
internal class LocalEventPublisher(
    IServiceProvider serviceProvider,
    LocalEventChannel eventChannel,
    IOptions<LocalEventOptions> options)
    : IEventPublisher
{
    /// <summary>
    /// 发布事件到本地事件管道（尽力而为语义：入队成功即返回，不保证被处理，见 <see cref="IEventPublisher"/>）；
    /// 仅拷贝会话标量字段快照，避免跨作用域捕获 Scoped 会话实例
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">待发布的事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="ArgumentNullException">事件为空时抛出。</exception>
    /// <exception cref="TimeoutException">在配置的发布超时内未获得队列空位时抛出。</exception>
    /// <exception cref="OperationCanceledException">调用方取消发布时抛出。</exception>
    /// <exception cref="System.Threading.Channels.ChannelClosedException">宿主停止后通道已关闭时抛出。</exception>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : EventBase => PublishAsync(@event, options.Value.PublishTimeout, cancellationToken);

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event, TimeSpan? timeout, CancellationToken cancellationToken = default)
        where TEvent : EventBase
    {
        Check.NotNull(@event, nameof(@event));
        var effectiveTimeout = timeout ?? options.Value.PublishTimeout;
        if (effectiveTimeout <= TimeSpan.Zero || effectiveTimeout.TotalMilliseconds > uint.MaxValue - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout,
                "发布超时必须大于零且不超过 4294967294 毫秒。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var session = serviceProvider.GetService<ISession>();
        var snapshot = session == null ? null : new SessionSnapshot(session);
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(effectiveTimeout);
        try
        {
            // 包括 Handler 重入发布在内，背压等待必须有界，避免唯一消费者等待自己的队列空位。
            await eventChannel.EventChannel.Writer.WriteAsync((snapshot, @event), timeoutSource.Token);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("本地事件发布超时：队列在配置的等待时间内持续满载。", exception);
        }
    }
}
