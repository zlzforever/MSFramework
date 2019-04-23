using System;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using MSFramework.EventSouring;
using Ordering.Domain.Aggregates;

namespace Ordering.Infrastructure.Repository
{
	public class OrderingRepository : EFAggrateRepository<Order, Guid>, IOrdingRepository
	{
		public OrderingRepository(DbContextFactory dbFactory, IEventStore eventStore) : base(dbFactory, eventStore)
		{
		}
	}
}