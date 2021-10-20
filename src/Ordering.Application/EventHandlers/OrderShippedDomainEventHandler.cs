using System.Threading.Tasks;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.EventHandlers
{
	public class OrderShippedDomainEventHandler : IMessageHandler<OrderShippedDomainEvent>
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

		public void Dispose()
		{
		}
	}
}