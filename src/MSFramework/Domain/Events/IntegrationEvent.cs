using System;
using MSFramework.Shared;

namespace MSFramework.Domain.Events
{
	public abstract class IntegrationEvent : IIntegrationEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public ObjectId EventId { get; private set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; private set; }

		protected IntegrationEvent() : this(ObjectId.NewId(), DateTimeOffset.Now)
		{
		}

		protected IntegrationEvent(ObjectId id, DateTimeOffset eventTime)
		{
			EventId = id;
			EventTime = eventTime;
		}
	}
}