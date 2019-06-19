using System;
using MSFramework.Common;

namespace MSFramework.EventBus
{
	public abstract class Event : IEvent
	{
		protected Event()
		{
			Id = CombGuid.NewGuid();
			Timestamp = DateTimeOffset.UtcNow;
		}

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset Timestamp { get; set; }

		public string Creator { get; set; }

		public string CreatorId { get; set; }
	}
}