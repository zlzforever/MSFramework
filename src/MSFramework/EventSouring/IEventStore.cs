using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFramework.EventSouring
{
	public interface IEventStore
	{
		Task<IEnumerable<EventHistory>> GetEventsAsync(string aggregateId, long from);

		Task AddEventAsync(params EventHistory[] events);
	}
}