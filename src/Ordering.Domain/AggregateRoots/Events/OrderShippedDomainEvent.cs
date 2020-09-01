using MicroserviceFramework.Domain.Events;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderShippedDomainEvent : Event
	{
		public Order Order { get; }

		public OrderShippedDomainEvent(Order order)
		{
			Order = order;
		}
	}
}