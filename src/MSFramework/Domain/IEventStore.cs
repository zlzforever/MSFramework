using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IEventStore
	{
		Task<IEnumerable<Event>> ReadEventsAsync(object id);

		Task<AppendResult> AppendEventAsync(IDomainEvent @event);
	}
}