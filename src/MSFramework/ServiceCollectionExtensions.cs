using System;
using System.Linq;
using EventBus.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Collections.Generic;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using MSFramework.Http;
using MSFramework.Reflection;

namespace MSFramework
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddEventBus(this MSFrameworkBuilder builder, params Type[] eventTypes)
		{
			builder.Services.AddEventBus(eventTypes);
			return builder;
		}

		public static void AddMSFramework(this IServiceCollection services,
			Action<MSFrameworkBuilder> builderAction = null)
		{
			var builder = new MSFrameworkBuilder(services);
			builderAction?.Invoke(builder);

			//初始化所有程序集查找器，如需更改程序集查找逻辑，请事先赋予自定义查找器的实例
			if (Singleton<IAssemblyFinder>.Instance == null)
			{
				Singleton<IAssemblyFinder>.Instance = new AssemblyFinder();
			}

			if (Singleton<IDependencyTypeFinder>.Instance == null)
			{
				Singleton<IDependencyTypeFinder>.Instance = new DependencyTypeFinder();
				var dependencyTypeDict = Singleton<IDependencyTypeFinder>.Instance.GetDependencyTypeDict();
				foreach (var kv in dependencyTypeDict)
				{
					builder.Services.Add(kv.Value, kv.Key);
				}
			}

			if (Singleton<IIdGenerator>.Instance == null)
			{
				Singleton<IIdGenerator>.Instance = new IdGenerator();
			}

			builder.AddInitializer();

			builder.Services.AddScoped<ScopedDictionary>();
			builder.Services.AddScoped<ApiClient>();
			builder.Services.AddMemoryCache();
			builder.Services.AddHttpClient();
		}

		public static IMSFrameworkApplicationBuilder UseMSFramework(this IServiceProvider applicationServices,
			Action<IMSFrameworkApplicationBuilder> configure = null)
		{
			Initialize(applicationServices);
			var builder = new MSFrameworkApplicationBuilder(applicationServices);
			configure?.Invoke(builder);

			applicationServices.UseEventBus();
			return builder;
		}

		private static void Initialize(IServiceProvider applicationServices)
		{
			var initializers = applicationServices.GetServices<Initializer>().OrderBy(x => x.Order).ToList();
			foreach (var initializer in initializers)
			{
				using var scope = applicationServices.CreateScope();
				initializer.Initialize(scope.ServiceProvider);
			}
		}
	}
}