using System;
using MSFramework.Common;

namespace MSFramework.Domain.Event
{
	public abstract class IntegrationEvent : IEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public Guid EventId { get; set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; set; }

 
		protected IntegrationEvent()
		{
			EventId = CombGuid.NewGuid();
			EventTime = DateTimeOffset.UtcNow;
		}
	}
}