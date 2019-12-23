using System.Threading.Tasks;
using MSFramework.Ef;
using MSFramework.EventBus;
using Ordering.Domain.AggregateRoot.Event;

namespace Ordering.Application.DomainEventHandler
{
	public class OrderShippedDomainEventHandler : IEventHandler<OrderShippedDomainEvent>
	{
		private readonly DbContextFactory _dbContextFactory;

		public OrderShippedDomainEventHandler(DbContextFactory dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public Task Handle(OrderShippedDomainEvent @event)
		{
			// todo
			return Task.CompletedTask;
		}
	}
}