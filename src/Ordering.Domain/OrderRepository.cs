using MSFramework.DependencyInjection;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain
{
	public interface IOrderReadRepository : IEfReadRepository<Order>
	{
	}

	public class OrderReadRepository : EfReadRepository<Order>, IScopeDependency
	{
		public OrderReadRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}

	public interface IOrderWriteRepository : IEfWriteRepository<Order>
	{
	}

	public class OrderWriteRepository : EfWriteRepository<Order>, IScopeDependency
	{
		public OrderWriteRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}