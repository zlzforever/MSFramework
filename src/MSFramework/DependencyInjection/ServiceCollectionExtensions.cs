using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSFramework.Application;
using MSFramework.Domain;
using MSFramework.Reflection;

namespace MSFramework.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 以类型实现的接口进行服务添加，需排除
		/// <see cref="IDisposable"/>等非业务接口，如无接口则注册自身
		/// </summary>
		/// <param name="services">服务映射信息集合</param>
		/// <param name="implementationTypes">要注册的实现类型集合</param>
		/// <param name="lifetime">注册的生命周期类型</param>
		internal static IServiceCollection Add(this IServiceCollection services,
			Type[] implementationTypes,
			ServiceLifetime lifetime)
		{
			foreach (Type implementationType in implementationTypes)
			{
				if (implementationType.IsAbstract || implementationType.IsInterface)
				{
					continue;
				}

				var excludeTypes = new[] {typeof(IApplicationService), typeof(IDomainService)};
				Type[] interfaceTypes = implementationType.GetImplementedInterfaces(typeof(IDisposable),
						typeof(ITransientDependency), typeof(ISingletonDependency), typeof(IScopeDependency))
					.Where(x => !excludeTypes.Contains(x)).ToArray();
				if (interfaceTypes.Length == 0)
				{
					services.TryAdd(new ServiceDescriptor(implementationType, implementationType, lifetime));
					continue;
				}

				for (int i = 0; i < interfaceTypes.Length; i++)
				{
					Type interfaceType = interfaceTypes[i];
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
						Type firstInterfaceType = interfaceTypes[0];
						services.TryAdd(new ServiceDescriptor(interfaceType,
							provider => provider.GetService(firstInterfaceType), lifetime));
					}
				}
			}

			return services;
		}
	}
}