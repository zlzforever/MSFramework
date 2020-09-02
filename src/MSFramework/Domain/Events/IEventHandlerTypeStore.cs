using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroserviceFramework.Domain.Events
{
	public interface IEventHandlerTypeStore
	{
		bool Add(Type eventType, Type handlerType);
		IReadOnlyDictionary<Type, MethodInfo> GetHandlerTypes(Type eventType);
	}
}