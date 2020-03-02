namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStartedDomainEvent : EventBus.Event
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