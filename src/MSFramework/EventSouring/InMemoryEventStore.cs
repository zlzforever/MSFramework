using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSFramework.EventSouring
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly Dictionary<Guid, List<StoredEvent>> _inMemoryDb =
			new Dictionary<Guid, List<StoredEvent>>();


		public Task<StoredEvent[]> GetEventsAsync(Guid aggregateRootId, long from)
		{
			return Task.FromResult(GetEvents(aggregateRootId, from));
		}

		public StoredEvent[] GetEvents(Guid aggregateRootId, long @from)
		{
			_inMemoryDb.TryGetValue(aggregateRootId, out var events);
			var entries = events != null ? events.Where(x => x.Version > from).ToArray() : new StoredEvent[0];
			return entries;
		}

		public Task AddEventsAsync(params StoredEvent[] events)
		{
			AddEvents(events);
			return Task.CompletedTask;
		}

		public void AddEvents(params StoredEvent[] events)
		{
			foreach (var @event in events)
			{
				_inMemoryDb.TryGetValue(@event.AggregateRootId, out var list);
				if (list == null)
				{
					list = new List<StoredEvent>();
					_inMemoryDb.Add(@event.AggregateRootId, list);
				}

				list.Add(@event);
			}
		}

		public Task<StoredEvent> GetLastEventAsync(Guid aggregateRootId)
		{
			return Task.FromResult(GetLastEvent(aggregateRootId));
		}

		public StoredEvent GetLastEvent(Guid aggregateRootId)
		{
			_inMemoryDb.TryGetValue(aggregateRootId, out var events);
			var @event = events?.LastOrDefault();
			return @event;
		}
	}
}