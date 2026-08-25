using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 工作单元
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// 注册保存事件
    /// </summary>
    event Func<CancellationToken, ValueTask> SavedChanges;

    /// <summary>
    /// 保存工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
