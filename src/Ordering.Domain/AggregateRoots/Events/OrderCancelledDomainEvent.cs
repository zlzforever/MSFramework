using MicroserviceFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderCancelledDomainEvent : DomainEvent
	{
		public Order Order { get; }

		public OrderCancelledDomainEvent(Order order)
		{
			Order = order;
		}
	}
}