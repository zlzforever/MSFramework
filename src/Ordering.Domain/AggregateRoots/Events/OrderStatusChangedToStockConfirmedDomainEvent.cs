using MSFramework.Common;
using MSFramework.Domain.Events;

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