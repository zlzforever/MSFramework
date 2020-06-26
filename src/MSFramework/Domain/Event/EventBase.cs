using System;
using MSFramework.Common;

namespace MSFramework.Domain.Event
{
	public abstract class EventBase : IEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public Guid EventId { get; private set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; private set; }

		/// <summary>
		/// 触发事件的对象
		/// </summary>
		public object EventSource { get; private set; }

		protected EventBase() : this(CombGuid.NewGuid(), DateTimeOffset.Now, null)
		{
		}

		protected EventBase(Guid id, DateTimeOffset eventTime, object eventSource)
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