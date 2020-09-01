using System;
using System.Collections.Generic;

namespace MicroserviceFramework.Domain.Events
{
	public interface IEventHandlerTypeStore
	{
		bool Add(Type eventType, Type handlerType);
		IEnumerable<Type> GetHandlerTypes(Type eventType);
	}
}