using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories
{
	public interface IOrderingRepository : IRepository<Order>, IScopeDependency
	{
	}
}