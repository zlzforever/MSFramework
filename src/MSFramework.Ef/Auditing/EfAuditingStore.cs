using System.Threading.Tasks;
using MicroserviceFramework.Auditing;

namespace MicroserviceFramework.Ef.Auditing;

public class EfAuditingStore : IAuditingStore
{
    private readonly DbContextFactory _dbContextFactory;

    public EfAuditingStore(DbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task AddAsync(AuditOperation auditOperation)
    {
        var dbContextBase = _dbContextFactory.GetDbContext<AuditOperation>();
        if (dbContextBase != null)
        {
            await dbContextBase.AddAsync(auditOperation);
        }
    }
}
