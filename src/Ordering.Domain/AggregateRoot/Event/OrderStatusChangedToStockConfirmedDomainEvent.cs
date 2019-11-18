using System;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: MSFramework.EventBus.Event
	{
		public Guid OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(Guid orderId)
			=> OrderId = orderId;
	}
}