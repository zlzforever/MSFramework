using System;

namespace MicroserviceFramework.EventBus
{
	public interface IIntegrationEventHandlerFactory
	{
		object Create(Type handlerType);
	}
}