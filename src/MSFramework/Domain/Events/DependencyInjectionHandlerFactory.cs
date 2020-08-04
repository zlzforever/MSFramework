using System;

namespace MSFramework.Domain.Events
{
	public class DependencyInjectionHandlerFactory : IHandlerFactory
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