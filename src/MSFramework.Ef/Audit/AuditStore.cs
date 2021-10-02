using System.Threading.Tasks;
using MicroserviceFramework.Audit;

namespace MicroserviceFramework.Ef.Audit
{
	public class AuditStore : IAuditStore
	{
		private readonly DbContextBase _dbContext;

		public AuditStore(DbContextFactory dbContextFactory)
		{
			_dbContext = dbContextFactory.GetDbContext<AuditOperation>();
		}

		public async Task AddAsync(AuditOperation auditOperation)
		{
			await _dbContext.AddAsync(auditOperation);
		}
	}
}