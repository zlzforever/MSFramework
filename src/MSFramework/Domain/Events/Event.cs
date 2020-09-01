using System;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain.Events
{
	public abstract class Event : IEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public ObjectId EventId { get; private set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; private set; }

		/// <summary>
		/// 触发事件的对象
		/// </summary>
		public object EventSource { get; private set; }

		protected Event() : this(ObjectId.NewId(), DateTimeOffset.Now, null)
		{
		}

		protected Event(ObjectId id, DateTimeOffset eventTime, object eventSource)
		{
			EventId = id;
			EventTime = eventTime;
			EventSource = eventSource;
		}

		public override string ToString()
		{
			return
				$"[{GetType().Name}] EventId = {EventId}, EventTime = {EventTime:YYYY-MM-DD HH:mm:ss}, EventSource = {EventSource}";
		}
	}
}