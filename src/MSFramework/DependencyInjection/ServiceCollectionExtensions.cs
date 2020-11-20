using System;
using System.Linq;
using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 以类型实现的接口进行服务添加，需排除
		/// <see cref="IDisposable"/>等非业务接口，如无接口则注册自身
		/// </summary>
		/// <param name="services">服务映射信息集合</param>
		/// <param name="implementationType">要注册的实现类型集合</param>
		/// <param name="lifetime">注册的生命周期类型</param>
		public static void RegisterDependencyInjection(this IServiceCollection services,
			Type implementationType, ServiceLifetime lifetime)
		{
			if (implementationType.IsAbstract || implementationType.IsInterface)
			{
				return;
			}

			// 1. 注册类型本身
			services.Add(new ServiceDescriptor(implementationType, implementationType, lifetime));

			var interfaceTypes = implementationType.GetInterfaces(
				typeof(ITransientDependency), typeof(ISingletonDependency), typeof(IScopeDependency),
				typeof(IApplicationService), typeof(IDomainService), typeof(IRepository)).ToArray();

			if (interfaceTypes.Length == 0)
			{
				return;
			}

			for (var i = 0; i < interfaceTypes.Length; i++)
			{
				var interfaceType = interfaceTypes[i];

				// 瞬时生命周期每次获取对象都是新的，因此实现的接口每个注册一次即可
				if (lifetime == ServiceLifetime.Transient)
				{
					services.Add(
						new ServiceDescriptor(interfaceType, implementationType, ServiceLifetime.Transient));
				}
				else
				{
					if (i == 0)
					{
						services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
					}
					else
					{
						//有多个接口时，后边的接口注册使用第一个接口的实例，保证同个实现类的多个接口获得同一个实例
						var firstInterfaceType = interfaceTypes[0];
						services.Add(new ServiceDescriptor(interfaceType,
							provider => provider.GetService(firstInterfaceType), lifetime));
					}
				}
			}
		}
	}
}