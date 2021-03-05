using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories
{
	public interface IOrderingRepository : IRepository<Order, ObjectId>, IScopeDependency
	{
	}
}