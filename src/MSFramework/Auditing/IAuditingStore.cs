using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;

namespace MicroserviceFramework.Auditing;

/// <summary>
/// 审记日志存储器
/// </summary>
public interface IAuditingStore
{
    /// <summary>
    /// 添加审计日志
    /// </summary>
    /// <param name="auditOperation"></param>
    /// <returns></returns>
    Task AddAsync(AuditOperation auditOperation);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Task CommitAsync();
}
