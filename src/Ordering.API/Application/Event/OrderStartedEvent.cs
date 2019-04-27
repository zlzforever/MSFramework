using System;
using MSFramework.Domain.Event;
using MSFramework.EventBus;
using MSFramework.IntegrateService;

namespace Ordering.API.Application.Event
{
	/// <summary>
	/// 发送到外部的领域事件
	/// </summary>
	public class OrderStartedEvent : IntegrationEvent
	{
		public string UserId { get; }
		
		public Guid OrderId { get; }

		public OrderStartedEvent(string userId, Guid orderId)
		{
			UserId = userId;
			OrderId = orderId;
		}
		
	}
}