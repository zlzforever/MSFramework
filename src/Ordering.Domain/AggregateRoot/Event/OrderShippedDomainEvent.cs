namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderShippedDomainEvent : MSFramework.EventBus.Event
	{
		public Order Order { get; }

		public OrderShippedDomainEvent(Order order)
		{
			Order = order;           
		}
	}
}