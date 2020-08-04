using MSFramework.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.Audits
{
	public interface IAuditRepository : IRepository<AuditOperation>, IScopeDependency
	{
	}
}