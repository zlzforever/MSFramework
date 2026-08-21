using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.LocalEvent;

internal class LocalEventPublisher(
    IServiceProvider serviceProvider,
    LocalEventChannel eventChannel)
    : IEventPublisher
{
    /// <summary>
    /// 发布事件到本地事件管道，仅拷贝会话标量字段快照，避免跨作用域捕获 Scoped 会话实例
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">待发布的事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : EventBase
    {
        Check.NotNull(@event, nameof(@event));
        var session = serviceProvider.GetService<ISession>();
        var snapshot = session == null ? null : new SessionSnapshot(session);
        await eventChannel.EventChannel.Writer.WriteAsync((snapshot, @event), cancellationToken);
    }
}
