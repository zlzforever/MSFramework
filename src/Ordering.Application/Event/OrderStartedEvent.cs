using System;
using MSFramework.Domain;

namespace Ordering.Application.Event
{
	/// <summary>
	/// 发送到外部的领域事件
	/// </summary>
	public class OrderStartedEvent : MSFramework.EventBus.Event
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