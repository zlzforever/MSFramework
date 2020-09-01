using System.Collections.Generic;
using MicroserviceFramework.Domain.Events;
using MicroserviceFramework.Shared;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderStatusChangedToAwaitingValidationDomainEvent
		: Event
	{
		public ObjectId OrderId { get; }
		public IEnumerable<OrderItem> OrderItems { get; }

		public OrderStatusChangedToAwaitingValidationDomainEvent(ObjectId orderId,
			IEnumerable<OrderItem> orderItems)
		{
			OrderId = orderId;
			OrderItems = orderItems;
		}
	}
}