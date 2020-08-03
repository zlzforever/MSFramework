using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToPaidDomainEvent
		: EventBase
	{
		public ObjectId OrderId { get; }
		public IEnumerable<OrderItem> OrderItems { get; }

		public OrderStatusChangedToPaidDomainEvent(ObjectId orderId,
			IEnumerable<OrderItem> orderItems)
		{
			OrderId = orderId;
			OrderItems = orderItems;
		}
	}
}