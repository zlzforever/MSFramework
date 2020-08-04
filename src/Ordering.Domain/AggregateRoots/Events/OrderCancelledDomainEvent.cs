using MSFramework.Domain.Events;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderCancelledDomainEvent : Event
	{
		public Order Order { get; }

		public OrderCancelledDomainEvent(Order order)
		{
			Order = order;
		}
	}
}