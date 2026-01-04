using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;

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
    ///
    /// </summary>
    /// <returns></returns>
    AuditOperation GetAuditOperation();

    /// <summary>
    /// 设置审计信息
    /// </summary>
    /// <param name="auditOperation"></param>
    void SetAuditOperation(AuditOperation auditOperation);

    /// <summary>
    /// 保存工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
