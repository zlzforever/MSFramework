using System;
using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: EventBase
	{
		public Guid OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(Guid orderId)
			=> OrderId = orderId;
	}
}