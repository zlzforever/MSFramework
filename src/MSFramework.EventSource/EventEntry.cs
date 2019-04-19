using System;
using System.Linq;
using MSFramework.Domain.Entity;
using MSFramework.EventBus;
using Newtonsoft.Json;

namespace MSFramework.EventSource
{
	public class EventEntry : EntityBase<int>
	{
		private EventEntry()
		{
		}

		public EventEntry(Event @event)
		{
			EventId = @event.Id;
			CreationTime = @event.CreationTime;
			EventTypeName = @event.GetType().FullName;
			Content = JsonConvert.SerializeObject(@event);
			Status = EventStatus.Ready;
			SentTimes = 0;
		}

		public Guid EventId { get; }

		public string EventTypeName { get; private set; }

		public EventStatus Status { get; set; }

		public int SentTimes { get; set; }

		public DateTime CreationTime { get; private set; }

		public string Content { get; private set; }

		public Event ToEvent()
		{
			var type = Type.GetType(EventTypeName);
			return JsonConvert.DeserializeObject(Content, type) as Event;
		}
	}
}