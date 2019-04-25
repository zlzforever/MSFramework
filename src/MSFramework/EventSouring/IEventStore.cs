using System;
using System.Threading.Tasks;

namespace MSFramework.EventSouring
{
	public interface IEventStore
	{
		Task<EventHistory[]> GetEventsAsync(Guid aggregateId, long from);

		EventHistory[] GetEvents(Guid aggregateId, long from);

		Task AddEventAsync(params EventHistory[] events);

		Task<EventHistory> GetLastEventAsync(Guid aggregateId);
		
		EventHistory GetLastEvent(Guid aggregateId);
	}
}