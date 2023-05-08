using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;

namespace MicroserviceFramework.Domain;

public interface IUnitOfWork
{
    void RegisterAuditing(Func<AuditOperation> auditingFactory);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void SaveChanges();
}
