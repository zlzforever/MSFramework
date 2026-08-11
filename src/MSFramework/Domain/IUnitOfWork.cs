using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 工作单元
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// 注册保存事件
    /// </summary>
    event Action SavedChanges;

    /// <summary>
    /// 订阅工作单元审计事件：触发工作单元订阅其内部所有数据上下文的保存事件，
    /// 用于在保存前收集审计实体。
    /// 本方法仅作订阅信号，审计操作本体统一从当前执行流的 <see cref="AuditOperationContext.Value"/> 读取，
    /// 不再通过参数传入；调用方需先向 <see cref="AuditOperationContext"/> 写入审计操作，
    /// 并在审计链路结束后将其置为 null 以清理执行流。
    /// </summary>
    void RegisterAuditOperation();

    /// <summary>
    /// 保存工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
