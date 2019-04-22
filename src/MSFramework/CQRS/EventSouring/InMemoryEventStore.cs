using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSFramework.Domain;
using MSFramework.EventBus;
using Newtonsoft.Json;

namespace MSFramework.CQRS.EventSouring
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly Dictionary<string, List<EventSourceEntry>> _inMemoryDb =
			new Dictionary<string, List<EventSourceEntry>>();


		public Task<IEnumerable<Event>> GetEventsAsync(string aggregateId, long from)
		{
			_inMemoryDb.TryGetValue(aggregateId, out var events);
			var entries = events != null
				? events.Where(x => x.Version > from).ToList()
				: new List<EventSourceEntry>();
			return Task.FromResult(
				entries.Select(x => (Event) JsonConvert.DeserializeObject(x.Event, Type.GetType(x.EventType)))
			);
		}

		public Task AddEventAsync(params EventSourceEntry[] events)
		{
			foreach (var @event in events)
			{
				_inMemoryDb.TryGetValue(@event.AggregateId, out var list);
				if (list == null)
				{
					list = new List<EventSourceEntry>();
					_inMemoryDb.Add(@event.AggregateId, list);
				}

				list.Add(@event);
			}

			return Task.CompletedTask;
		}
	}
}