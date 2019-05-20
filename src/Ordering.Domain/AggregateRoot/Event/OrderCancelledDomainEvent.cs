using MediatR;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderCancelledDomainEvent : INotification
	{
		public Order Order { get; }

		public OrderCancelledDomainEvent(Order order)
		{
			Order = order;
		}
	}
}