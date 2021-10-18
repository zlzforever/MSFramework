using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.DependencyInjection
{
	public static class LifetimeUtilities
	{
		public static ServiceLifetime? GetLifetime(Type type)
		{
			if (type.IsAbstract || type.IsInterface)
			{
				return null;
			}

			if (typeof(ISingletonDependency).IsAssignableFrom(type))
			{
				return ServiceLifetime.Singleton;
			}

			if (typeof(IScopeDependency).IsAssignableFrom(type))
			{
				return ServiceLifetime.Scoped;
			}

			if (typeof(ITransientDependency).IsAssignableFrom(type))
			{
				return ServiceLifetime.Transient;
			}

			return null;
		}
	}
}