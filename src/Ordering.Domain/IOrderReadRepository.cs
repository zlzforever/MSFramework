using MSFramework.DependencyInjection;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain
{
	public interface IOrderReadRepository : IReadRepository<Order>, IScopeDependency
	{
	}


	public interface IOrderWriteRepository : IWriteRepository<Order>, IScopeDependency
	{
	}
}