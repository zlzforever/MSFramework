using System;
using MSFramework.Common;
using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: EventBase
	{
		public ObjectId OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(ObjectId orderId)
			=> OrderId = orderId;
	}
}