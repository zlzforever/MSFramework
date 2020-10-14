using System;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.EventBus
{
	public abstract class Event
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public ObjectId EventId { get; private set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public DateTimeOffset EventTime { get; private set; }

		protected Event()
		{
			EventId = ObjectId.NewId();
			EventTime = DateTimeOffset.Now;
		}

		public override string ToString()
		{
			return
				$"[{GetType().Name}] EventId = {EventId}, EventTime = {EventTime:YYYY-MM-DD HH:mm:ss}";
		}
	}
}