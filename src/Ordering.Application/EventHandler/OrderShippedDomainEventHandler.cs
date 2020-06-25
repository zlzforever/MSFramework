using System.Threading.Tasks;
using MSFramework.Domain.Event;
using MSFramework.Ef;
using Ordering.Domain.AggregateRoot.Event;

namespace Ordering.Application.EventHandler
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