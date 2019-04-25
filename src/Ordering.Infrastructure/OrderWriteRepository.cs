using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Infrastructure
{
	public class OrderWriteRepository : EfWriteRepository<Order>, IOrderWriteRepository
	{
		public OrderWriteRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}