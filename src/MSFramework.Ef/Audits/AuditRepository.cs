using MSFramework.Audits;
using MSFramework.Ef.Repositories;

namespace MSFramework.Ef.Audits
{
	public class AuditRepository : EfRepository<AuditOperation>, IAuditRepository
	{
		public AuditRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}