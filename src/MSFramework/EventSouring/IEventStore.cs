using System.Threading.Tasks;

namespace MSFramework.EventSouring
{
	public interface IEventStore
	{
		Task<EventHistory[]> GetEventsAsync(string aggregateId, long from);

		EventHistory[] GetEvents(string aggregateId, long from);

		Task AddEventAsync(params EventHistory[] events);
		
		void AddEvents(params EventHistory[] events);

		Task<EventHistory> GetLastEventAsync(string aggregateId);
		
		EventHistory GetLastEvent(string aggregateId);
	}
}