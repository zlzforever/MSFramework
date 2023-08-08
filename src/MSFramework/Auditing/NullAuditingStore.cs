using System.Threading.Tasks;

namespace MicroserviceFramework.Auditing;

internal class NullAuditingStore : IAuditingStore
{
    public Task AddAsync(AuditOperation auditOperation)
    {
        return Task.CompletedTask;
    }
}
