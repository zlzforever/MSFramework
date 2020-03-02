using System;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: EventBus.Event
	{
		public Guid OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(Guid orderId)
			=> OrderId = orderId;
	}
}