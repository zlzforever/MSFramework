using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Auditing;

/// <summary>
///
/// </summary>
/// <param name="dbContext"></param>
/// <typeparam name="TDbContext"></typeparam>
public class EfAuditingStore<TDbContext>(TDbContext dbContext) : IAuditingStore
    where TDbContext : DbContext
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="auditOperation"></param>
    public async Task AddAsync(AuditOperation auditOperation)
    {
        await dbContext.AddAsync(auditOperation);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public Task CommitAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
