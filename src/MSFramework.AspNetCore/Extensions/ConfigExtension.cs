using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSFramework.Application;
using MSFramework.Extensions;

namespace MSFramework.AspNetCore.Extensions
{
	public static class ConfigExtension
	{
		public static IServiceCollection AddConfigModel(this IServiceCollection services, Assembly assembly)
		{
			var typeInfos = assembly.GetTypes().Where(t => t.GetCustomAttributes<ConfigModelAttribute>().Any())
				.Select(i => new
				{
					TypeInfo = i,
					ConfigAttribute = i.GetCustomAttributes<ConfigModelAttribute>().First()
				}).ToArray();

			foreach (var i in typeInfos)
			{
				services.Add(new ServiceDescriptor(i.TypeInfo, provider =>
				{
					var configSection = provider.GetService<IConfiguration>()
						.GetSection(i.ConfigAttribute.SectionName.IsNullOrEmpty()
							? i.TypeInfo.Name
							: i.ConfigAttribute.SectionName);
					if (i.ConfigAttribute.IsOptional)
					{
						return configSection.Get(i.TypeInfo) ?? Activator.CreateInstance(i.TypeInfo);
					}

					return configSection.Get(i.TypeInfo);
				}, i.ConfigAttribute.IsAllowReload ? ServiceLifetime.Transient : ServiceLifetime.Singleton));
			}

			return services;
		}
	}
}