using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Reflection;

namespace MSFramework.Initializer
{
	public static class InitializerServiceCollectionExtensions
	{
		internal static MSFrameworkBuilder UseInitializer(this MSFrameworkBuilder builder)
		{
			var assemblies = AssemblyFinder.GetAllList();
			var types = new HashSet<Type>();
			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					types.Add(type);
				}
			}

			var initializerType = typeof(InitializerBase);
			var notAllowAutoRegisterInitializerType = typeof(INotAutoRegisterInitializer);
			types.Remove(initializerType);
			foreach (var type in types)
			{
				if (initializerType.IsAssignableFrom(type) &&
				    !notAllowAutoRegisterInitializerType.IsAssignableFrom(type))
				{
					builder.Services.AddSingleton(initializerType, type);
				}
			}

			return builder;
		}

		public static MSFrameworkBuilder AddInitializer<TInitializer>(this MSFrameworkBuilder builder)
			where TInitializer : InitializerBase
		{
			builder.Services.AddSingleton<TInitializer>();
			return builder;
		}

		public static IServiceCollection AddInitializer<TInitializer>(this IServiceCollection serviceCollection)
			where TInitializer : InitializerBase
		{
			serviceCollection.AddSingleton<InitializerBase, TInitializer>();
			return serviceCollection;
		}
	}
}