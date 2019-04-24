using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSFramework.EventSouring
{
	public class InMemoryEventStore : IEventStore
	{
		private readonly Dictionary<Guid, List<EventHistory>> _inMemoryDb =
			new Dictionary<Guid, List<EventHistory>>();


		public Task<IEnumerable<EventHistory>> GetEventsAsync(Guid aggregateId, long from)
		{
			_inMemoryDb.TryGetValue(aggregateId, out var events);
			var entries = events != null ? events.Where(x => x.Version > from) : Enumerable.Empty<EventHistory>();
			return Task.FromResult(entries);
		}

		public Task AddEventAsync(params EventHistory[] events)
		{
			foreach (var @event in events)
			{
				_inMemoryDb.TryGetValue(@event.AggregateId, out var list);
				if (list == null)
				{
					list = new List<EventHistory>();
					_inMemoryDb.Add(@event.AggregateId, list);
				}

				list.Add(@event);
			}

			return Task.CompletedTask;
		}
	}
}