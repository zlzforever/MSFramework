using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Reflection;

namespace MSFramework.Common
{
	public static class InitializerServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddInitializer(this MSFrameworkBuilder builder)
		{
			var assemblies = Singleton<IAssemblyFinder>.Instance.GetAllAssemblyList();
			var types = new List<Type>();
			foreach (var assembly in assemblies)
			{
				types.AddRange(assembly.GetTypes());
			}

			var initializerType = typeof(Initializer);
			types.Remove(initializerType);
			foreach (var type in types)
			{
				if (initializerType.IsAssignableFrom(type))
				{
					builder.Services.AddSingleton(initializerType, type);
				}
			}

			return builder;
		}
	}
}