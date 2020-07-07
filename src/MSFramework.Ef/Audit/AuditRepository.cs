using MSFramework.Audit;
using MSFramework.Ef.Repository;

namespace MSFramework.Ef.Audit
{
	public class AuditRepository : EfRepository<AuditOperation>, IAuditRepository
	{
		public AuditRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}