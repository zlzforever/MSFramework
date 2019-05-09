using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using MSFramework.EventBus;
using MSFramework.EventSouring;
using MSFramework.Reflection;
using MSFramework.Serialization;

namespace MSFramework
{
	public static class ServiceCollectionExtensions
	{
		private static ICollection<Tuple<Type, Type>> _eventHandlers = new List<Tuple<Type, Type>>();
		private static ICollection<Tuple<string, Type>> _dynamicEventHandlers = new List<Tuple<string, Type>>();

		public static MSFrameworkBuilder AddEventHandler<TEvent, TEventHandler>(this MSFrameworkBuilder builder)
			where TEvent : class, IEvent
			where TEventHandler : IEventHandler<TEvent>
		{
			var type = typeof(TEventHandler);
			builder.Services.AddScoped(type);
			_eventHandlers.Add(new Tuple<Type, Type>(typeof(TEvent), type));
			return builder;
		}

		public static MSFrameworkBuilder AddEventHandler<TEventHandler>(this MSFrameworkBuilder builder, string name)
			where TEventHandler : IDynamicEventHandler
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new MSFrameworkException("Subscribe name should not be empty/null");
			}

			var type = typeof(TEventHandler);
			builder.Services.AddScoped(type);
			_dynamicEventHandlers.Add(new Tuple<string, Type>(name, type));

			return builder;
		}

		public static MSFrameworkBuilder UseInMemoryEventStore(this MSFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
			return builder;
		}

		public static MSFrameworkBuilder UseNewtonsoftJsonConvert(this MSFrameworkBuilder builder)
		{
			Singleton<IJsonConvert>.Instance = new NewtonsoftJsonConvert(new JsonConvertOptions());
			builder.Services.AddSingleton(Singleton<IJsonConvert>.Instance);
			return builder;
		}

		public static IServiceProvider AddMSFramework(this IServiceCollection services,
			Action<MSFrameworkBuilder> builderAction = null)
		{
			var builder = new MSFrameworkBuilder(services);
			builderAction?.Invoke(builder);

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

			if (Singleton<IJsonConvert>.Instance == null)
			{
				builder.UseNewtonsoftJsonConvert();
			}

			if (Singleton<IIdGenerator>.Instance == null)
			{
				Singleton<IIdGenerator>.Instance = new IdGenerator();
			}

			builder.Services.AddSingleton<IEventBusSubscriptionStore, InMemoryEventBusSubscriptionStore>();
			builder.Services.AddSingleton<IPassThroughEventBus, PassThroughEventBus>();
			builder.Services.AddHttpClient();
			builder.UseLocalEventBus();

			return services.BuildAspectInjectorProvider();
		}

		public static IApplicationBuilder UseMSFramework(this IApplicationBuilder builder)
		{
			var eventBus = builder.ApplicationServices.GetService<IEventBus>();
			var passEventBus = builder.ApplicationServices.GetRequiredService<IPassThroughEventBus>();

			var subscribeMethod = typeof(IEventBus).GetMethod("Subscribe", new Type[0]);
			if (subscribeMethod != null)
			{
				var noneParameters = new object[0];

				foreach (var handler in _eventHandlers)
				{
					var method = subscribeMethod.MakeGenericMethod(handler.Item1, handler.Item2);
					if (eventBus != null)
					{
						method.Invoke(eventBus, noneParameters);
					}

					method.Invoke(passEventBus, noneParameters);
				}
			}

			var subscribeDynamicMethod = typeof(IEventBus).GetMethod("Subscribe", new[] {typeof(string)});
			if (subscribeDynamicMethod != null)
			{
				foreach (var handler in _dynamicEventHandlers)
				{
					var typeParameters = new object[] {handler.Item1};
					if (eventBus != null)
					{
						subscribeDynamicMethod.Invoke(eventBus, typeParameters);
					}

					subscribeDynamicMethod.Invoke(passEventBus, typeParameters);
				}
			}

			_eventHandlers = new List<Tuple<Type, Type>>();
			_dynamicEventHandlers = new List<Tuple<string, Type>>();

			var initializers = builder.ApplicationServices.GetServices<IInitializer>().ToList();
			foreach (var initializer in initializers)
			{
				initializer.Initialize();
			}

			return builder;
		}
	}
}