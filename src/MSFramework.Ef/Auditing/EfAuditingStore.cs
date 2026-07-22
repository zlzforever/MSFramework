using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Auditing;

/// <summary>
/// EF Core 审计存储实现，将审计操作持久化到数据库
/// </summary>
/// <param name="dbContext">数据库上下文</param>
/// <typeparam name="TDbContext">DbContext 类型</typeparam>
public class EfAuditingStore<TDbContext>(TDbContext dbContext) : IAuditingStore
    where TDbContext : DbContext
{
    /// <summary>
    /// 异步添加审计操作记录
    /// </summary>
    /// <param name="auditOperation">审计操作信息</param>
    public async Task AddAsync(AuditOperation auditOperation)
    {
        await dbContext.AddAsync(auditOperation);
        await dbContext.SaveChangesAsync();
    }
}
