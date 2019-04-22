using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IEventStore
	{
		Task<IEnumerable<Event>> GetEventsAsync(string aggregateId, long from);

		Task AddEventAsync(params EventSourceEntry[] @event);
	}
}