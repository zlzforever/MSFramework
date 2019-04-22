using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.Domain;
using MSFramework.EventBus;

namespace MSFramework.CQRS.EventSouring
{
	public interface IEventStore
	{
		Task<IEnumerable<Event>> GetEventsAsync(string aggregateId, long from);

		Task AddEventAsync(params EventSourceEntry[] @event);
	}
}