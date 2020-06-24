using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderShippedDomainEvent : EventBase
	{
		public Order Order { get; }

		public OrderShippedDomainEvent(Order order)
		{
			Order = order;
		}
	}
}