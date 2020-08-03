using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.Events;

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