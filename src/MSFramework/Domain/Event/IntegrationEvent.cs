using System;
using MSFramework.Common;

namespace MSFramework.Domain.Event
{
	public abstract class IntegrationEvent : IIntegrationEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public Guid EventId { get; private set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; private set; }

		protected IntegrationEvent() : this(CombGuid.NewGuid(), DateTimeOffset.Now)
		{
		}

		protected IntegrationEvent(Guid id, DateTimeOffset eventTime)
		{
			EventId = id;
			EventTime = eventTime;
		}
	}
}