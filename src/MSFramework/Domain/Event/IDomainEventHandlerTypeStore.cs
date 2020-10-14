using System;
using System.Collections.Generic;

namespace MicroserviceFramework.Domain.Event
{
	public interface IDomainEventHandlerTypeStore
	{
		void Add(Type eventType, Type handlerType);
		IReadOnlyCollection<DomainEventHandlerInfo> GetHandlers(Type eventType);
	}
}