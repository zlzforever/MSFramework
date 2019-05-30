using MediatR;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderShippedDomainEvent : INotification
	{
		public Order Order { get; }

		public OrderShippedDomainEvent(Order order)
		{
			Order = order;           
		}
	}
}