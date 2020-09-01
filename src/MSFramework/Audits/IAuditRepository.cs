using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Audits
{
	public interface IAuditRepository : IRepository<AuditOperation>, IScopeDependency
	{
	}
}