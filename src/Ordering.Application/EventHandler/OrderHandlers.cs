using System.Threading.Tasks;
using MSFramework.EntityFrameworkCore;
using MSFramework.EventBus;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.EventHandler
{
	public class OrderHandlers : IEventHandler<OrderCreatedEvent>
	{
		private readonly DbContextFactory _dbContextFactory;

		public OrderHandlers(DbContextFactory dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public async Task Handle(OrderCreatedEvent @event)
		{
			await _dbContextFactory.GetDbContext<Order>().Set<Order>().AddAsync(new Order(@event.UserId, @event.Address,
				@event.Description, @event.OrderItems));
		}
	}
}