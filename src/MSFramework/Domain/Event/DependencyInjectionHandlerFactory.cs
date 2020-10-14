using System;

namespace MicroserviceFramework.Domain.Event
{
	public class DependencyInjectionHandlerFactory : IDomainHandlerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public DependencyInjectionHandlerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public object Create(Type handlerType)
		{
			return _serviceProvider.GetService(handlerType);
		}
	}
}