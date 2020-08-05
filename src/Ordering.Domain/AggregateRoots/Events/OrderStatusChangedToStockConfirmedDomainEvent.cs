using MSFramework.Domain.Events;
using MSFramework.Shared;

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