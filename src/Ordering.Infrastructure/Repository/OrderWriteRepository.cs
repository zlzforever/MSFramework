using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class OrderWriteRepository : EfWriteRepository<Order>, IOrderWriteRepository
	{
		public OrderWriteRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}