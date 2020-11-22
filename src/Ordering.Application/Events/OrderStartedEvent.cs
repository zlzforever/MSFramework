using System;
using MicroserviceFramework.EventBus;

namespace Ordering.Application.Events
{
	/// <summary>
	/// 发送到外部的领域事件
	/// </summary>
	public class OrderStartedEvent :  EventBase
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