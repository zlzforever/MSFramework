using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using MSFramework.EventBus;
using MSFramework.Reflection;

namespace MSFramework
{
	public static class ServiceCollectionExtensions
	{
		private static Type[] _types;

		public static MSFrameworkBuilder AddEventHandler(this MSFrameworkBuilder builder, params Type[] eventTypes)
		{
			_types = eventTypes;

			if (_types != null && _types.Length > 0)
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

			LoadBizAssemblies();
			//初始化所有程序集查找器，如需更改程序集查找逻辑，请事先赋予自定义查找器的实例
			if (Singleton<IAssemblyFinder>.Instance == null)
			{
				Singleton<IAssemblyFinder>.Instance = new AssemblyFinder();
				builder.Services.AddSingleton(Singleton<IAssemblyFinder>.Instance);
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

			builder.Services.AddSingleton<IEventBusSubscriptionStore, InMemoryEventBusSubscriptionStore>();

			builder.Services.AddHttpClient();
		}

		public static IApplicationBuilder UseMSFramework(this IApplicationBuilder builder)
		{
			Singleton<IServiceProvider>.Instance = builder.ApplicationServices;
			SubscribeEventHandler(builder);
			Initialize(builder);
			return builder;
		}

		private static void SubscribeEventHandler(IApplicationBuilder builder)
		{
			using var scope = builder.ApplicationServices.CreateScope();
			var eventBus = scope.ServiceProvider.GetService<IEventBus>();
			if (_types != null && _types.Length > 0 && eventBus != null)
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

		private static void Initialize(IApplicationBuilder builder)
		{
			var initializers = builder.ApplicationServices.GetServices<Initializer>().ToList();
			initializers.Sort((x, y) => x.Order < y.Order ? 0 : 1);
			foreach (var initializer in initializers)
			{
				initializer.Initialize(builder);
			}
		}

		/// <summary>
		/// 引用的项目如果未使用具体类型，此程序集不会加载到当前应用程序域中，因此把业务项目的程序集全加载。
		/// </summary>
		private static void LoadBizAssemblies()
		{
			DependencyContext context = DependencyContext.Default;
			var assemblyNames = context.RuntimeLibraries.Where(x => x.Type == "project")
				.SelectMany(x => x.GetDefaultAssemblyNames(DependencyContext.Default)).ToList();
			foreach (var assemblyName in assemblyNames)
			{
				Assembly.Load(assemblyName);
			}
		}
	}
}