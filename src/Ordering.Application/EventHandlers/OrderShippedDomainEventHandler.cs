using System.Threading.Tasks;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Ef;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.EventHandlers
{
	public class OrderShippedDomainEventHandler : IDomainEventHandler<OrderShippedDomainEvent>
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