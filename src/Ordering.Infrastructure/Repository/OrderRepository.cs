using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class OrderRepository
		: EfRepository<Order>, IOrderingRepository
	{
		public OrderRepository(DbContextFactory context) : base(context)
		{
		}
	}
}