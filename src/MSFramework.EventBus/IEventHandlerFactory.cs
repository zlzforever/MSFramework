using System;

namespace MicroserviceFramework.EventBus
{
	public interface IEventHandlerFactory
	{
		object Create(Type handlerType);
	}
}