using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSFramework.EventSouring
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly Dictionary<string, List<EventHistory>> _inMemoryDb =
			new Dictionary<string, List<EventHistory>>();


		public Task<EventHistory[]> GetEventsAsync(string aggregateId, long from)
		{
			return Task.FromResult(GetEvents(aggregateId, from));
		}

		public EventHistory[] GetEvents(string aggregateId, long @from)
		{
			_inMemoryDb.TryGetValue(aggregateId, out var events);
			var entries = events != null ? events.Where(x => x.Version > from).ToArray() : new EventHistory[0];
			return entries;
		}

		public Task AddEventsAsync(params EventHistory[] events)
		{
			AddEvents(events);
			return Task.CompletedTask;
		}

		public void AddEvents(params EventHistory[] events)
		{
			foreach (var @event in events)
			{
				_inMemoryDb.TryGetValue(@event.AggregateRootId, out var list);
				if (list == null)
				{
					list = new List<EventHistory>();
					_inMemoryDb.Add(@event.AggregateRootId, list);
				}

				list.Add(@event);
			}
		}

		public Task<EventHistory> GetLastEventAsync(string aggregateId)
		{
			return Task.FromResult(GetLastEvent(aggregateId));
		}

		public EventHistory GetLastEvent(string aggregateId)
		{
			_inMemoryDb.TryGetValue(aggregateId, out var events);
			var @event = events?.LastOrDefault();
			return @event;
		}
	}
}