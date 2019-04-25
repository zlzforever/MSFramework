using System;

namespace MSFramework.Domain.Event
{
	public class DeletedEvent : AggregateEventBase
	{
		public static Type Type = typeof(DeletedEvent);
	}
}