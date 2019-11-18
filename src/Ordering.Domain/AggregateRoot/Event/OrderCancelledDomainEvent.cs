namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderCancelledDomainEvent : MSFramework.EventBus.Event
	{
		public Order Order { get; }

		public OrderCancelledDomainEvent(Order order)
		{
			Order = order;
		}
	}
}