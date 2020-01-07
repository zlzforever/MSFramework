using System;
using MSFramework.Common;

namespace MSFramework.EventBus
{
	public abstract class Event : IEvent
	{
		protected Event()
		{
			EventId = CombGuid.NewGuid();
			Timestamp = DateTimeOffset.UtcNow;
		}

		public Guid EventId { get; set; }

		public int Version { get; set; }

		public DateTimeOffset Timestamp { get; set; }

		public string CreationUserName { get; set; }

		public string CreationUserId { get; set; }
	}
}