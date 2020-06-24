using System;
using System.Collections.Generic;
using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToAwaitingValidationDomainEvent
		: EventBase
	{
		public Guid OrderId { get; }
		public IEnumerable<OrderItem> OrderItems { get; }

		public OrderStatusChangedToAwaitingValidationDomainEvent(Guid orderId,
			IEnumerable<OrderItem> orderItems)
		{
			OrderId = orderId;
			OrderItems = orderItems;
		}
	}
}