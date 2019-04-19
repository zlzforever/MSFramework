using System;

namespace Ordering.Domain.Events
{
	using MediatR;

	/// <summary>
	/// Event used when the order stock items are confirmed
	/// </summary>
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: INotification
	{
		public Guid OrderId { get; }

		public OrderStatusChangedToStockConfirmedDomainEvent(Guid orderId)
			=> OrderId = orderId;
	}
}