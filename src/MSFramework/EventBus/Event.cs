using System;
using MSFramework.Common;

namespace MSFramework.EventBus
{
	public abstract class Event : IEvent
	{
		private string _creator;

		protected Event()
		{
			Id = CombGuid.NewGuid();
			Timestamp = DateTime.UtcNow;
		}

		public Guid Id { get; }

		public DateTime Timestamp { get; }

		public string Creator => _creator;

		public void SetCreator(string creator)
		{
			_creator = creator;
		}
	}
}