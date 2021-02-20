using System;
using System.Collections.Generic;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.DependencyInjection
{
	public static class ServiceLocator
	{
		private static IServiceProvider _serviceProvider;

		public static IServiceProvider ServiceProvider
		{
			get => _serviceProvider;
			internal set
			{
				Check.NotNull(value, nameof(ServiceProvider));
				_serviceProvider = value;
			}
		}

		public static T GetService<T>()
		{
			Check.NotNull(ServiceProvider, nameof(ServiceProvider));

			var scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
			return scopedResolver != null ? scopedResolver.GetService<T>() : ServiceProvider.GetService<T>();
		}

		public static object GetService(Type serviceType)
		{
			Check.NotNull(ServiceProvider, nameof(ServiceProvider));

			var scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
			return scopedResolver != null
				? scopedResolver.GetService(serviceType)
				: ServiceProvider.GetService(serviceType);
		}

		public static IEnumerable<T> GetServices<T>()
		{
			Check.NotNull(ServiceProvider, nameof(ServiceProvider));

			var scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
			return scopedResolver != null ? scopedResolver.GetServices<T>() : ServiceProvider.GetServices<T>();
		}

		public static IEnumerable<object> GetServices(Type serviceType)
		{
			Check.NotNull(ServiceProvider, nameof(ServiceProvider));

			var scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
			return scopedResolver != null
				? scopedResolver.GetServices(serviceType)
				: ServiceProvider.GetServices(serviceType);
		}
	}
}