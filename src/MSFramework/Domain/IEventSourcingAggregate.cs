using System;
using System.Collections.Generic;
using MediatR;
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