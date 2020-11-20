using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.DependencyInjection
{
	public class LifetimeChecker
	{
		private static readonly Type Scope = typeof(IScopeDependency);
		private static readonly Type Singleton = typeof(ISingletonDependency);
		private static readonly Type Transient = typeof(ITransientDependency);

		public static ServiceLifetime? Get(Type type)
		{
			if (type.IsAbstract || type.IsInterface)
			{
				return null;
			}

			if (Singleton.IsAssignableFrom(type))
			{
				return ServiceLifetime.Scoped;
			}

			if (Scope.IsAssignableFrom(type))
			{
				return ServiceLifetime.Scoped;
			}

			if (Transient.IsAssignableFrom(type))
			{
				return ServiceLifetime.Scoped;
			}

			return null;
		}
	}
}