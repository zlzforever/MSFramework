using System;

namespace Ordering.API.Application.Event
{
	public class OrderStatusChangedToShippedIntegrationEvent : MSFramework.EventBus.Event
	{
		public Guid OrderId { get; }
		public string OrderStatus { get; }
		public string BuyerName { get; }

		public OrderStatusChangedToShippedIntegrationEvent(Guid orderId, string orderStatus, string buyerName)
		{
			OrderId = orderId;
			OrderStatus = orderStatus;
			BuyerName = buyerName;
		}
	}
}