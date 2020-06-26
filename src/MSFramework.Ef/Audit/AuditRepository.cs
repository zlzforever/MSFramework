using MSFramework.Audit;
using MSFramework.Ef.Repository;

namespace MSFramework.Ef.Audit
{
	public class AuditRepository : EfRepository<AuditedOperation>, IAuditRepository
	{
		public AuditRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}