using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;

namespace MicroserviceFramework.Ef.Auditing;

public class EfAuditingStore<TDbContext>(TDbContext dbContext) : IAuditingStore
    where TDbContext : DbContextBase
{
    public async Task AddAsync(AuditOperation auditOperation)
    {
        await dbContext.AddAsync(auditOperation);
    }

    public Task CommitAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
