using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Audit
{
	public interface IAuditRepository : IRepository<AuditOperation>, IScopeDependency
	{
	}
}