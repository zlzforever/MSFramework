using System;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.AggregateRoot.Order;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class BuyerRepository : EfRepository<Order, Guid>, IOrderingRepository
	{
		public BuyerRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}