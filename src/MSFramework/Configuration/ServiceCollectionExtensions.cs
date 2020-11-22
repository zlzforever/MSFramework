using System;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Configuration
{
	public static class ServiceCollectionExtensions
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
					var configuration = provider.GetRequiredService<IConfiguration>();
					var constructor = type.GetConstructor(new[] {typeof(IConfiguration)});
					var result = constructor == null
						? Activator.CreateInstance(type)
						: Activator.CreateInstance(type, configuration);

					if (attribute.SectionName.IsNullOrEmpty())
					{
						configuration.Bind(result);
					}
					else
					{
						var section = provider.GetRequiredService<IConfiguration>().GetSection(attribute.SectionName);
						section.Bind(result);
					}

					return result;
				}, attribute.ReloadOnChange ? ServiceLifetime.Transient : ServiceLifetime.Singleton));
			}

			return services;
		}

		public static void Print(this IConfiguration configuration, Action<string> writer)
		{
			if (configuration == null || writer == null)
			{
				return;
			}

			writer("Configuration: ");
			foreach (var kv in configuration.GetChildren())
			{
				if (!string.IsNullOrWhiteSpace(kv.Key))
				{
					writer($"{kv.Key} = {kv.Value}");
				}
			}
		}
	}
}