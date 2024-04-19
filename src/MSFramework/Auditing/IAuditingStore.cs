using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;

namespace MicroserviceFramework.Auditing;

/// <summary>
/// 审记日志存储器
/// </summary>
public interface IAuditingStore
{
    Task AddAsync(AuditOperation auditOperation);
}
