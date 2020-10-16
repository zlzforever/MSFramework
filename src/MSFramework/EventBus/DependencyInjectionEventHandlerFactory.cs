using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.EventBus
{
	public class DependencyInjectionEventHandlerFactory : IEventHandlerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public DependencyInjectionEventHandlerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public object Create(Type handlerType)
		{
			var scope = _serviceProvider.CreateScope();
			return scope.ServiceProvider.GetService(handlerType);
		}
	}
}