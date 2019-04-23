using System;
using MSFramework.Domain;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.AggregateRoot.Order;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class OrderingRepository : EfRepository<Order, Guid>, IOrderingRepository
	{
		public OrderingRepository(DbContextFactory dbContextFactory) : base(
			dbContextFactory
		)
		{
		}
	}
}