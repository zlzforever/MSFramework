using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Shared;

namespace Ordering.Domain.AggregateRoots.Events
{
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: DomainEvent
	{
		public ObjectId OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(ObjectId orderId)
			=> OrderId = orderId;
	}
}