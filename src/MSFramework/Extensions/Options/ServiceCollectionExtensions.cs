using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Extensions.Options;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    private static void AddOptionsType(this IServiceCollection services, Type optionsType,
        IConfiguration config,
        Action<BinderOptions> configureBinder)
    {
        // ArgumentNullException.ThrowIfNull(services);

        if (optionsType.IsAbstract || !optionsType.IsClass)
        {
            throw new ArgumentException("配置类型必需是类");
        }

        // ArgumentNullException.ThrowIfNull(config);

        var configurationChangeTokenSourceType =
            typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionsType);
        var configurationChangeTokenSource =
            Activator.CreateInstance(configurationChangeTokenSourceType, string.Empty, config);
        if (configurationChangeTokenSource == null)
        {
            throw new ArgumentNullException(nameof(configurationChangeTokenSource));
        }

        services.TryAddSingleton(typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionsType),
            _ => configurationChangeTokenSource);

        var namedConfigureFromConfigurationOptionsType =
            typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionsType);
        var namedConfigureFromConfigurationOptions =
            Activator.CreateInstance(namedConfigureFromConfigurationOptionsType, string.Empty, config,
                configureBinder);
        if (namedConfigureFromConfigurationOptions == null)
        {
            throw new ArgumentNullException(nameof(namedConfigureFromConfigurationOptions));
        }

        services.TryAddSingleton(typeof(IConfigureOptions<>).MakeGenericType(optionsType),
            _ => namedConfigureFromConfigurationOptions);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddOptionsType(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        foreach (var type in Utils.Runtime.GetAllTypes())
        {
            if (!(!type.IsAbstract && type.IsClass))
            {
                continue;
            }

            var attribute = type.GetCustomAttribute<OptionsTypeAttribute>();
            if (attribute == null)
            {
                continue;
            }

            var section = string.IsNullOrWhiteSpace(attribute.Section)
                ? configuration
                : configuration.GetSection(attribute.Section);
            services.AddOptionsType(type, section, _ => { });
        }

        return services;
    }
}
