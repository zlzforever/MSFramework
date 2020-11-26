using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Configuration
{
	public static class ServiceCollectionExtensions
	{
		private static IServiceCollection AddOptions(this IServiceCollection services, Type optionsType, string name,
			IConfiguration config,
			Action<BinderOptions> configureBinder)
		{
			if (!optionsType.IsClass)
			{
				throw new ArgumentException("optionsType should be class");
			}

			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			services.AddOptions();

			var configurationChangeTokenSourceType =
				typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionsType);
			var configurationChangeTokenSource =
				Activator.CreateInstance(configurationChangeTokenSourceType, name, config);
			if (configurationChangeTokenSource == null)
			{
				throw new ArgumentNullException(nameof(configurationChangeTokenSource));
			}

			services.AddSingleton(typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionsType),
				configurationChangeTokenSource);

			var namedConfigureFromConfigurationOptionsType =
				typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionsType);
			var namedConfigureFromConfigurationOptions =
				Activator.CreateInstance(namedConfigureFromConfigurationOptionsType, name, config, configureBinder);
			if (namedConfigureFromConfigurationOptions == null)
			{
				throw new ArgumentNullException(nameof(namedConfigureFromConfigurationOptions));
			}

			return services.AddSingleton(typeof(IConfigureOptions<>).MakeGenericType(optionsType),
				namedConfigureFromConfigurationOptions);
		}

		public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
		{
			MicroserviceFrameworkLoader.RegisterType += type =>
			{
				if (type.IsAbstract || type.IsInterface)
				{
					return;
				}

				var attribute = type.GetCustomAttribute<OptionsAttribute>();
				if (attribute != null)
				{
					services.AddOptions(type, attribute.SectionName, configuration, _ => { });
				}
			};

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