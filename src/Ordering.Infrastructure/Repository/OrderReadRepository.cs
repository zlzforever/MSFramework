using System;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class OrderReadRepository : EfReadRepository<Order,Guid>,IOrderReadRepository
	{
		public OrderReadRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}