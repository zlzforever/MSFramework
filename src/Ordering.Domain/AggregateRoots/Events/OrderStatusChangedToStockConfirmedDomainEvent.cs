using MicroserviceFramework.Domain.Events;
using MicroserviceFramework.Shared;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: Event
	{
		public ObjectId OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(ObjectId orderId)
			=> OrderId = orderId;
	}
}