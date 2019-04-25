using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Infrastructure
{
	public class OrderReadRepository : EfReadRepository<Order>,IOrderReadRepository
	{
		public OrderReadRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}