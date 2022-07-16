using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
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

		public void Dispose()
		{
		}

		public Task HandleAsync(OrderShippedDomainEvent query, CancellationToken cancellationToken = default)
		{
			// todo
			return Task.CompletedTask;
		}
	}
}