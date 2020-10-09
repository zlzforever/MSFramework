using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroserviceFramework.Domain.Events
{
	public interface IEventHandlerTypeStore
	{
		bool Add(string eventType, Type handlerType);
		IReadOnlyDictionary<Type, MethodInfo> GetHandlerTypes(string eventType);
	}
}