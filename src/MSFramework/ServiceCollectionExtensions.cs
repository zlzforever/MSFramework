using System;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Command;
using MSFramework.Common;
using MSFramework.DependencyInjection;
using MSFramework.Domain.Repository;
using MSFramework.EventBus;
using MSFramework.EventSouring;
using MSFramework.Reflection;
using MSFramework.Security;
using MSFramework.Serialization;

namespace MSFramework
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddInMemoryEventStore(this MSFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
			return builder;
		}

		public static IServiceCollection AddMSFramework(this IServiceCollection services,
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
				Singleton<IJsonConvert>.Instance = new DefaultJsonConvert(new JsonConvertOptions());
				builder.Services.AddSingleton(Singleton<IJsonConvert>.Instance);
			}

			builder.Services.AddMediatR(Singleton<IAssemblyFinder>.Instance.GetAllAssemblyList().ToArray());
			builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			builder.Services.AddSingleton<ICurrentUser, CurrentUser>();
			builder.Services.AddSingleton<ICommandBus, CommandBus>();
			builder.Services.AddLocalEventBus();

			return services;
		}
	}
}