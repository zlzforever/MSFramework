using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderCancelledDomainEvent : EventBase
	{
		public Order Order { get; }

		public OrderCancelledDomainEvent(Order order)
		{
			Order = order;
		}
	}
}