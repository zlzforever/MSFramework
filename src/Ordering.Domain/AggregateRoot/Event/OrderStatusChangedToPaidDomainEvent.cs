using System;
using System.Collections.Generic;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToPaidDomainEvent
		: MSFramework.EventBus.Event
	{
		public Guid OrderId { get; }
		public IEnumerable<OrderItem> OrderItems { get; }

		public OrderStatusChangedToPaidDomainEvent(Guid orderId,
			IEnumerable<OrderItem> orderItems)
		{
			OrderId = orderId;
			OrderItems = orderItems;
		}
	}
}