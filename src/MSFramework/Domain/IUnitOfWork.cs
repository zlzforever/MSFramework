using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 工作单元
/// </summary>
public interface IUnitOfWork
{
    event Func<Task> SavedChanges;

    /// <summary>
    /// 设置审计信息
    /// </summary>
    /// <param name="factory"></param>
    void SetAuditOperationFactory(Func<AuditOperation> factory);

    /// <summary>
    /// 保存工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
