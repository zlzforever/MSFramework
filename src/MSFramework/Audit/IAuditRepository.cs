using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Audit
{
	public interface IAuditRepository : IRepository<AuditOperation, ObjectId>, IScopeDependency
	{
	}
}