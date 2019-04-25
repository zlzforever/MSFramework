using MSFramework.DependencyInjection;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain.Repository
{

	public interface IOrderWriteRepository : IWriteRepository<Order>, IScopeDependency
	{
	}
}