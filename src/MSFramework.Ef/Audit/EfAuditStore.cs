using System.Threading.Tasks;
using MicroserviceFramework.Audit;

namespace MicroserviceFramework.Ef.Audit;

/// <summary>
/// EF audit store 不应该和业务共用同一个 Scope 的 db context
/// </summary>
public class EfAuditStore : IAuditStore
{
    private readonly DbContextBase _dbContext;

    public EfAuditStore(DbContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.GetDbContext<AuditOperation>();
    }

    public async Task AddAsync(AuditOperation auditOperation)
    {
        await _dbContext.AddAsync(auditOperation);
    }

    public async Task FlushAsync()
    {
        await _dbContext.CommitAsync();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
