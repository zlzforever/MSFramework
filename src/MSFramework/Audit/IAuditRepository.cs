using MSFramework.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.Audit
{
	public interface IAuditRepository : IRepository<AuditedOperation>, IScopeDependency
	{
	}
}