using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;

namespace MicroserviceFramework.Auditing;

public interface IAuditingStore
{
    Task AddAsync(AuditOperation auditOperation);
    // Task CommitAsync();
}
