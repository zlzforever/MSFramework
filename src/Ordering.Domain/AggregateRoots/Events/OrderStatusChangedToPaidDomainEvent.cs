using System.Collections.Generic;
using MicroserviceFramework.Domain.Event;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderStatusChangedToPaidDomainEvent
		: DomainEvent
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