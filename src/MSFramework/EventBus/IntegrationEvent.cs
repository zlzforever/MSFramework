using System;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.EventBus
{
	public class IntegrationEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public ObjectId EventId { get; private set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; private set; }

		public IntegrationEvent() : this(ObjectId.NewId(), DateTimeOffset.Now)
		{
		}

		public IntegrationEvent(ObjectId id, DateTimeOffset eventTime)
		{
			EventId = id;
			EventTime = eventTime;
		}

		public override string ToString()
		{
			return
				$"[{GetType().Name}] EventId = {EventId}, EventTime = {EventTime:YYYY-MM-DD HH:mm:ss}";
		}
	}
}