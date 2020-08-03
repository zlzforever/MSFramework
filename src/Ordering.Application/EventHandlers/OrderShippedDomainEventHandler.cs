using System.Threading.Tasks;
using MSFramework.Domain.Events;
using MSFramework.Ef;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.EventHandlers
{
	public class OrderShippedDomainEventHandler : IEventHandler<OrderShippedDomainEvent>
	{
		private readonly DbContextFactory _dbContextFactory;

		public OrderShippedDomainEventHandler(DbContextFactory dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public Task HandleAsync(OrderShippedDomainEvent @event)
		{
			// todo
			return Task.CompletedTask;
		}
	}
}