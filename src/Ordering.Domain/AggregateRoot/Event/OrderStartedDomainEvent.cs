namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStartedDomainEvent : MSFramework.EventBus.Event
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