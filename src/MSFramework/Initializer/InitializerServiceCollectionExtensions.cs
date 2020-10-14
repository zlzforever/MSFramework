using System;
using System.Collections.Generic;
using MicroserviceFramework.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Initializer
{
	public static class InitializerServiceCollectionExtensions
	{
		internal static MicroserviceFrameworkBuilder UseInitializer(this MicroserviceFrameworkBuilder builder)
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
			var nonAutomaticInitializerType = typeof(INotAutomaticRegisterInitializer);
			types.Remove(initializerType);
			foreach (var type in types)
			{
				if (initializerType.IsAssignableFrom(type) &&
				    !nonAutomaticInitializerType.IsAssignableFrom(type))
				{
					builder.Services.AddSingleton(initializerType, type);
				}
			}

			return builder;
		}

		public static MicroserviceFrameworkBuilder AddInitializer<TInitializer>(this MicroserviceFrameworkBuilder builder)
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