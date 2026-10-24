using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 本地事件处理耗尽重试后的死信。
/// </summary>
public sealed record LocalEventDeadLetter(
    EventBase EventData,
    Type HandlerType,
    int Attempts,
    Exception Exception,
    DateTimeOffset FailedAt);

/// <summary>
/// 本地事件死信存储。生产环境可替换为持久实现并接入重放工具。
/// </summary>
public interface ILocalEventDeadLetterStore
{
    /// <summary>
    /// 存储处理失败的本地事件。
    /// </summary>
    /// <param name="deadLetter">死信内容</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    Task StoreAsync(LocalEventDeadLetter deadLetter, CancellationToken cancellationToken = default);
}

/// <summary>
/// 默认进程内死信存储，提供可观测性；进程重启恢复需替换为持久实现。
/// </summary>
public sealed class InMemoryLocalEventDeadLetterStore : ILocalEventDeadLetterStore
{
    private readonly ConcurrentQueue<LocalEventDeadLetter> _entries = new();

    /// <summary>
    /// 获取当前进程内保存的死信快照。
    /// </summary>
    public IReadOnlyCollection<LocalEventDeadLetter> Entries => _entries.ToArray();

    /// <inheritdoc />
    public Task StoreAsync(LocalEventDeadLetter deadLetter, CancellationToken cancellationToken = default)
    {
        _entries.Enqueue(deadLetter);
        return Task.CompletedTask;
    }
}
