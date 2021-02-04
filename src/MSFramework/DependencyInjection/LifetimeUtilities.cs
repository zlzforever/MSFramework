using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.DependencyInjection
{
	public static class LifetimeUtilities
	{
		private static readonly Type Scope = typeof(IScopeDependency);
		private static readonly Type Singleton = typeof(ISingletonDependency);
		private static readonly Type Transient = typeof(ITransientDependency);

		public static ServiceLifetime? GetLifetime(Type type)
		{
			if (type.IsAbstract || type.IsInterface)
			{
				return null;
			}

			if (Singleton.IsAssignableFrom(type))
			{
				return ServiceLifetime.Singleton;
			}

			if (Scope.IsAssignableFrom(type))
			{
				return ServiceLifetime.Scoped;
			}

			if (Transient.IsAssignableFrom(type))
			{
				return ServiceLifetime.Transient;
			}

			return null;
		}
	}
}