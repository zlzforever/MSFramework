// using System;
// using System.Collections.Generic;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace MicroserviceFramework.EventBus
// {
// 	public class DependencyInjectionEventHandlerFactory : IEventHandlerFactory
// 	{
// 		private readonly IServiceProvider _serviceProvider;
//
// 		public DependencyInjectionEventHandlerFactory(IServiceProvider serviceProvider)
// 		{
// 			_serviceProvider = serviceProvider;
// 		}
//
// 		public IEnumerable<object> Create(Type handlerType)
// 		{
// 			using var scope = _serviceProvider.CreateScope();
// 			return scope.ServiceProvider.GetServices(handlerType);
// 		}
// 	}
// }