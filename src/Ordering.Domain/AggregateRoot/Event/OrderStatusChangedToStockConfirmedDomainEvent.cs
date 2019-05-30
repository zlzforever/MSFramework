using System;
using MediatR;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: INotification
	{
		public Guid OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(Guid orderId)
			=> OrderId = orderId;
	}
}