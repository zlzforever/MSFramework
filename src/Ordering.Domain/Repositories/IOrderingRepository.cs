using MSFramework.DependencyInjection;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories
{
	public interface IOrderingRepository : IRepository<Order>, IScopeDependency
	{
	}
}