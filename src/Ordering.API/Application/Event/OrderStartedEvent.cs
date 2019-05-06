using System;
using MSFramework.Domain;

namespace Ordering.API.Application.Event
{
	/// <summary>
	/// 发送到外部的领域事件
	/// </summary>
	public class OrderStartedEvent : DistributedDomainEvent
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