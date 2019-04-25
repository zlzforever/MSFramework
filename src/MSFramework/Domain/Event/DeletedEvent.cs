using System;

namespace MSFramework.Domain.Event
{
	public class DeletedEvent : AggregateEventBase
	{
		public static readonly Type Type = typeof(DeletedEvent);
	}
}