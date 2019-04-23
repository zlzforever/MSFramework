using MSFramework.CQRS.EventSouring;
using MSFramework.Domain.Repository;
using System;
using MSFramework.Domain;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repository
{
	public class OrderingRepository : EFAggrateRepository<Order, Guid>, IOrdingRepository
	{
		public OrderingRepository(DbContextFactory dbFactory, IEventStore eventStore) : base(dbFactory, eventStore)
		{
		}
	}
}