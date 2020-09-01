using MicroserviceFramework.Audits;
using MicroserviceFramework.Ef.Repositories;

namespace MicroserviceFramework.Ef.Audits
{
	public class AuditRepository : EfRepository<AuditOperation>, IAuditRepository
	{
		public AuditRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}