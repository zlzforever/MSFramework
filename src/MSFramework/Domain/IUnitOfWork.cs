using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;

namespace MicroserviceFramework.Domain;

public interface IUnitOfWork
{
    void SetAuditingFactory(Func<AuditOperation> factory);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // void SaveChanges();

    /// <summary>
    /// 注册操作
    /// </summary>
    /// <param name="action"></param>
    void Register(Func<Task> action);
}
