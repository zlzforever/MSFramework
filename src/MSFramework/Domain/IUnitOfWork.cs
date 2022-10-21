using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Audit;

namespace MicroserviceFramework.Domain;

public interface IUnitOfWork
{
    IEnumerable<AuditEntity> GetAuditEntities();

    void RegisterAuditOperation(AuditOperation auditOperation);

    Task CommitAsync();

    /// <summary>
    /// 调用所有 DbContext 的 SaveChanges，没有额外如事件发布的操作
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();
}
