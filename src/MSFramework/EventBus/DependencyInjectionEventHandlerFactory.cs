using System;
using System.Collections.Generic;
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

		public IEnumerable<object> Create(Type handlerType)
		{
			return _serviceProvider.CreateScope().ServiceProvider.GetServices(handlerType);
		}
	}
}