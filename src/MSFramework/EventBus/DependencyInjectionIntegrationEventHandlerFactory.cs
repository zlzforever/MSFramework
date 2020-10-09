using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.EventBus
{
	public class DependencyInjectionIntegrationEventHandlerFactory : IIntegrationEventHandlerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public DependencyInjectionIntegrationEventHandlerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public object Create(Type handlerType)
		{
			using var scope = _serviceProvider.CreateScope();
			return scope.ServiceProvider.GetService(handlerType);
		}
	}
}