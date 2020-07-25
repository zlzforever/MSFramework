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
		public static IServiceCollection AddConfigType(this IServiceCollection services, Assembly assembly)
		{
			var typeInfos = assembly.GetTypes()
				.Where(type => type.GetCustomAttributes<ConfigTypeAttribute>().Any())
				.Select(x => new
				{
					Type = x,
					Attribute = x.GetCustomAttributes<ConfigTypeAttribute>().First()
				}).ToArray();

			foreach (var typeInfo in typeInfos)
			{
				var type = typeInfo.Type;
				var attribute = typeInfo.Attribute;
				services.TryAdd(new ServiceDescriptor(type, provider =>
				{
					var configSection = provider.GetService<IConfiguration>()
						.GetSection(attribute.SectionName.IsNullOrEmpty()
							? type.Name
							: attribute.SectionName);
					if (attribute.Optional)
					{
						return configSection.Get(type) ?? Activator.CreateInstance(type);
					}

					return configSection.Get(type);
				}, attribute.ReloadOnChange ? ServiceLifetime.Transient : ServiceLifetime.Singleton));
			}

			return services;
		}
	}
}