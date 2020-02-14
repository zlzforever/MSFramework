using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Audit;
using MSFramework.Collections.Generic;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using MSFramework.EventBus;
using MSFramework.Http;
using MSFramework.Reflection;

namespace MSFramework
{
	public static class ServiceCollectionExtensions
	{
		private static HashSet<Type> _types = new HashSet<Type>();

		public static MSFrameworkBuilder AddEventHandler(this MSFrameworkBuilder builder, params Type[] eventTypes)
		{
			foreach (var type in eventTypes)
			{
				_types.Add(type);
			}

			_types.Add(typeof(AuditOperationEvent));

			if (_types != null && _types.Count > 0)
			{
				var types = _types.SelectMany(x => x.Assembly.GetTypes()).ToList();
				var baseEventType = typeof(Event);
				var dynamicHandler = typeof(IDynamicEventHandler);

				foreach (var handlerType in types)
				{
					if (dynamicHandler.IsAssignableFrom(handlerType))
					{
						var subscribeName = handlerType.GetCustomAttribute<SubscribeName>();
						if (subscribeName != null)
						{
							builder.Services.AddTransient(handlerType);
						}
					}
					else
					{
						var eventType = handlerType.GetInterface("IEventHandler`1")?.GenericTypeArguments
							.SingleOrDefault();
						if (eventType != null && baseEventType.IsAssignableFrom(baseEventType))
						{
							builder.Services.AddTransient(handlerType);
						}
					}
				}
			}

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

			builder.Services.AddSingleton<IEventBusSubscriptionStore, InMemoryEventBusSubscriptionStore>();
			builder.Services.AddScoped<ScopedDictionary>();
			builder.Services.AddScoped<IBearProvider, DefaultBearProvider>();
			builder.Services.AddScoped<ApiClient>();
			builder.Services.AddHttpClient();
		}

		public static IMSFrameworkApplicationBuilder UseMSFramework(this IServiceProvider applicationServices,
			Action<IMSFrameworkApplicationBuilder> configure = null)
		{
			Singleton<IServiceProvider>.Instance = applicationServices;
			SubscribeEventHandler(applicationServices);
			Initialize(applicationServices);
			var builder = new MSFrameworkApplicationBuilder(applicationServices);
			configure?.Invoke(builder);

			return builder;
		}

		private static void SubscribeEventHandler(IServiceProvider applicationServices)
		{
			using var scope = applicationServices.CreateScope();
			var eventBus = scope.ServiceProvider.GetService<IEventBus>();
			if (_types != null && _types.Count > 0 && eventBus != null)
			{
				var subscribeHandlerMethod = typeof(IEventBus).GetMethod("Subscribe", new Type[0]);
				if (subscribeHandlerMethod == null)
				{
					return;
				}

				var subscribeDynamicHandlerMethod = typeof(IEventBus).GetMethod("Subscribe", new[] {typeof(string)});
				if (subscribeDynamicHandlerMethod == null)
				{
					return;
				}

				var types = _types.SelectMany(x => x.Assembly.GetTypes()).ToList();
				var baseEventType = typeof(Event);
				var dynamicHandler = typeof(IDynamicEventHandler);

				foreach (var handlerType in types)
				{
					if (dynamicHandler.IsAssignableFrom(handlerType))
					{
						var subscribeName = handlerType.GetCustomAttribute<SubscribeName>();
						if (subscribeName != null)
						{
							subscribeDynamicHandlerMethod.MakeGenericMethod(handlerType).Invoke(eventBus,
								new object[] {subscribeName.Name});
						}
					}
					else
					{
						var eventType = handlerType.GetInterface("IEventHandler`1")?.GenericTypeArguments
							.SingleOrDefault();
						if (eventType != null && baseEventType.IsAssignableFrom(baseEventType))
						{
							var method = subscribeHandlerMethod.MakeGenericMethod(eventType, handlerType);
							method.Invoke(eventBus, new object[0]);
						}
					}
				}
			}
		}

		private static void Initialize(IServiceProvider applicationServices)
		{
			var initializers = applicationServices.GetServices<Initializer>().ToList();
			initializers.Sort((x, y) => x.Order - y.Order);
			foreach (var initializer in initializers)
			{
				initializer.Initialize(applicationServices);
			}
		}
	}
}