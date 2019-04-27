using System;

namespace MSFramework.EventBus
{
	public interface IEvent
	{
		Guid Id { get; }

		DateTime Timestamp { get; }
		
		string Creator { get; }
		
		void SetCreator(string creator);
	}
}