using System;
using System.Collections.Generic;

namespace MicroserviceFramework.Domain.Event
{
	public interface IEventHandlerTypeStore
	{
		void Add(Type eventType, Type handlerType);
		IReadOnlyCollection<HandlerInfo> GetHandlers(Type eventType);
	}
}