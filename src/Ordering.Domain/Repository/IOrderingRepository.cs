using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain.Repository
{
	public interface IOrderingRepository : IRepository<Order>
	{
	}
}