using MSFramework.Domain.Events;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderStartedDomainEvent : Event
	{
		public string UserId { get; }

		public Order Order { get; }

		public OrderStartedDomainEvent(Order order, string userId )
		{
			Order = order;
			UserId = userId;
		}
	}
}