using MSFramework.DependencyInjection;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain.Repository
{
	public interface IOrderingRepository : IRepository<Order>, IScopeDependency
	{
	}
}