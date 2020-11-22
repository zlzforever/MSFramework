using System;
using System.Collections.Generic;

namespace MicroserviceFramework.EventBus
{
	public interface IEventHandlerFactory
	{
		IEnumerable<object> Create(Type handlerType);
	}
}