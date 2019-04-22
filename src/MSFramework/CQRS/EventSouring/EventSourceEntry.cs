using System;
using MSFramework.EventBus;
using Newtonsoft.Json;

namespace MSFramework.Domain
{
	public class EventSourceEntry
	{
		public string EventType { get; }

		public string Event { get; }

		public string AggregateId { get; }

		public long Version { get; }

		public EventSourceEntry(IAggregateEvent @event)
		{
			EventType = @event.GetType().Name;
			Version = @event.Version;
			Event = JsonConvert.SerializeObject(@event);
			AggregateId = @event.IdAsString();
		}
	}
}