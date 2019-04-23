using System.Collections.Generic;
using MSFramework.Domain.Event;

namespace MSFramework.Domain
{
	public interface IEventSourcingAggregate
	{
		long Version { get; }

		IEnumerable<IAggregateEvent> GetAggregateEvents();

		void ClearAggregateEvents();
	}
}