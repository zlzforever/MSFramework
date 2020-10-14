using MicroserviceFramework.Audit;
using MicroserviceFramework.Ef.Repositories;

namespace MicroserviceFramework.Ef.Audit
{
	public class AuditRepository : EfRepository<AuditOperation>, IAuditRepository
	{
		public AuditRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}