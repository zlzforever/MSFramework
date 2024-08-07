using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Auditing;

public class EfAuditingStore<TDbContext>(TDbContext dbContext) : IAuditingStore
    where TDbContext : DbContext
{
    public async Task AddAsync(AuditOperation auditOperation)
    {
        await dbContext.AddAsync(auditOperation);
    }

    // public Task CommitAsync()
    // {
    //     return dbContext.SaveChangesAsync();
    // }
}
