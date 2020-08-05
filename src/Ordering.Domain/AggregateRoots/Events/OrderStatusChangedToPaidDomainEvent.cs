using System.Collections.Generic;
using MSFramework.Domain.Events;
using MSFramework.Shared;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderStatusChangedToPaidDomainEvent
		: Event
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