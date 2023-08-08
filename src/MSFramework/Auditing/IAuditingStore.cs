using System.Threading.Tasks;

namespace MicroserviceFramework.Auditing;

public interface IAuditingStore
{
    Task AddAsync(AuditOperation auditOperation);
}
