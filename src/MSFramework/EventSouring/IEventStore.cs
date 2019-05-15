using System;
using System.Threading.Tasks;

namespace MSFramework.EventSouring
{
	public interface IEventStore
	{
		Task<StoredEvent[]> GetEventsAsync(Guid aggregateRootId, long from);

		StoredEvent[] GetEvents(Guid aggregateRootId, long from);

		Task AddEventsAsync(params StoredEvent[] events);
		
		void AddEvents(params StoredEvent[] events);

		Task<StoredEvent> GetLastEventAsync(Guid aggregateRootId);
		
		StoredEvent GetLastEvent(Guid aggregateRootId);
	}
}