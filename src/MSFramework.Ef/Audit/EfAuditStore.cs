using System.Threading.Tasks;
using MicroserviceFramework.Audit;

namespace MicroserviceFramework.Ef.Audit
{
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
}