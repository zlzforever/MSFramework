using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot.Event;

namespace Ordering.Application.DomainEventHandler
{
	public class OrderShippedDomainEventHandler : INotificationHandler<OrderShippedDomainEvent>
	{
		private readonly DbContextFactory _dbContextFactory;

		public OrderShippedDomainEventHandler(DbContextFactory dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken)
		{
			// todo
			return Task.CompletedTask;
		}
	}
}