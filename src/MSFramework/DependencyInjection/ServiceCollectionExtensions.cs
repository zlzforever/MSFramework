using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSFramework.Application;
using MSFramework.Domain;
using MSFramework.Extensions;

namespace MSFramework.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseDependencyInjectionScanner(this MSFrameworkBuilder builder)
		{
			var dependencyTypeDict = DependencyTypeFinder.GetDependencyTypeDict();
			foreach (var kv in dependencyTypeDict)
			{
				builder.Services.Add(kv.Value, kv.Key);
			}

			return builder;
		}

		/// <summary>
		/// 以类型实现的接口进行服务添加，需排除
		/// <see cref="IDisposable"/>等非业务接口，如无接口则注册自身
		/// </summary>
		/// <param name="services">服务映射信息集合</param>
		/// <param name="implementationTypes">要注册的实现类型集合</param>
		/// <param name="lifetime">注册的生命周期类型</param>
		private static void Add(this IServiceCollection services,
			Type[] implementationTypes,
			ServiceLifetime lifetime)
		{
			foreach (var implementationType in implementationTypes)
			{
				if (implementationType.IsAbstract || implementationType.IsInterface)
				{
					continue;
				}

				var excludeTypes = new[] {typeof(IApplicationService), typeof(IDomainService)};
				var interfaceTypes = implementationType.GetImplementedInterfaces(typeof(IDisposable),
						typeof(ITransientDependency), typeof(ISingletonDependency), typeof(IScopeDependency))
					.Where(x => !excludeTypes.Contains(x)).ToArray();
				if (interfaceTypes.Length == 0)
				{
					services.TryAdd(new ServiceDescriptor(implementationType, implementationType, lifetime));
					continue;
				}

				for (var i = 0; i < interfaceTypes.Length; i++)
				{
					var interfaceType = interfaceTypes[i];
					if (lifetime == ServiceLifetime.Transient)
					{
						services.TryAddEnumerable(new ServiceDescriptor(interfaceType, implementationType, lifetime));
						continue;
					}

					if (i == 0)
					{
						services.TryAdd(new ServiceDescriptor(interfaceType, implementationType, lifetime));
					}
					else
					{
						//有多个接口时，后边的接口注册使用第一个接口的实例，保证同个实现类的多个接口获得同一个实例
						var firstInterfaceType = interfaceTypes[0];
						services.Add(new ServiceDescriptor(interfaceType,
							provider =>
							{
								var service = provider.GetService(firstInterfaceType);
								return service;
							}, lifetime));
					}
				}
			}
		}
	}
}