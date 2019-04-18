using System;

namespace MSFramework.EventBus
{
	public interface IEvent
	{
		Guid Id { get; }

		DateTime CreationTime { get; }
	}
}