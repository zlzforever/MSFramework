using System;
using System.Collections.Generic;
using MSFramework.Domain.Event;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IEventSourcingAggregate
	{
		long Version { get; }

		IEnumerable<IAggregateEvent> GetAggregateEvents();

		void ClearAggregateEvents();
	}
}