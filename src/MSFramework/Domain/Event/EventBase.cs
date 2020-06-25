using System;
using MSFramework.Common;

namespace MSFramework.Domain.Event
{
	public abstract class EventBase : IEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public Guid EventId { get; set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; set; }

		/// <summary>
		/// 触发事件的对象
		/// </summary>
		public object EventSource { get; set; }

		protected EventBase() : this(null)
		{
		}

		protected EventBase(object eventSource)
		{
			EventId = CombGuid.NewGuid();
			EventTime = DateTimeOffset.UtcNow;
			EventSource = eventSource;
		}
	}
}