using MicroserviceFramework.Audit;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Ef.Audit
{
	public class AuditRepository : EfRepository<AuditOperation, ObjectId>, IAuditRepository
	{
		public AuditRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}