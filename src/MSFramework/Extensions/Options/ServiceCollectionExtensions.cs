using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Extensions.Options;

public static class ServiceCollectionExtensions
{
    private static void AddOptionsType(this IServiceCollection services, Type optionsType,
        IConfiguration config,
        Action<BinderOptions> configureBinder)
    {
        if (!optionsType.IsClass)
        {
            throw new ArgumentException("options type should be class");
        }

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var configurationChangeTokenSourceType =
            typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionsType);
        var configurationChangeTokenSource =
            Activator.CreateInstance(configurationChangeTokenSourceType, string.Empty, config);
        if (configurationChangeTokenSource == null)
        {
            throw new ArgumentNullException(nameof(configurationChangeTokenSource));
        }

        services.AddSingleton(typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionsType),
            configurationChangeTokenSource);

        var namedConfigureFromConfigurationOptionsType =
            typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionsType);
        var namedConfigureFromConfigurationOptions =
            Activator.CreateInstance(namedConfigureFromConfigurationOptionsType, string.Empty, config,
                configureBinder);
        if (namedConfigureFromConfigurationOptions == null)
        {
            throw new ArgumentNullException(nameof(namedConfigureFromConfigurationOptions));
        }

        services.AddSingleton(typeof(IConfigureOptions<>).MakeGenericType(optionsType),
            namedConfigureFromConfigurationOptions);
    }

    public static IServiceCollection AddOptionsType(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        MicroserviceFrameworkLoaderContext.Get(services).ResolveType += type =>
        {
            if (type.IsAbstract || type.IsInterface)
            {
                return;
            }

            var attribute = type.GetCustomAttribute<OptionsTypeAttribute>();
            if (attribute == null)
            {
                return;
            }

            var section = string.IsNullOrWhiteSpace(attribute.Section)
                ? configuration
                : configuration.GetSection(attribute.Section);
            services.AddOptionsType(type, section, _ => { });
        };

        return services;
    }
}
