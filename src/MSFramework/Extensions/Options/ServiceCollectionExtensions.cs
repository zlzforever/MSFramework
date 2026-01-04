using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Extensions.Options;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="services"></param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 不适合对所有 IServiceCollection 开放，若没有 Utils.Runtime 支持，注册不进去
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal IServiceCollection AddOptionsType(IConfiguration configuration)
        {
            services.AddOptions();

            foreach (var type in Utils.Runtime.GetAllTypes())
            {
                if (!(!type.IsAbstract && type.IsClass))
                {
                    continue;
                }

                var attribute = type.GetCustomAttribute<AutoOptionsAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                services.AddOptionsType(type, attribute, configuration);
            }

            return services;
        }

        private void AddOptionsType(Type optionsType, AutoOptionsAttribute attribute, IConfiguration config)
        {
            if (optionsType.IsAbstract || !optionsType.IsClass)
            {
                throw new ArgumentException("配置类型必需是类");
            }

            var name = attribute.Name;
            var section = string.IsNullOrWhiteSpace(attribute.Section) ? config : config.GetSection(attribute.Section);
            var configurationChangeTokenSourceType =
                typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionsType);
            var configurationChangeTokenSource =
                Activator.CreateInstance(configurationChangeTokenSourceType, name, section);
            if (configurationChangeTokenSource == null)
            {
                throw new ArgumentNullException(nameof(configurationChangeTokenSource));
            }

            services.AddSingleton(typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionsType),
                _ => configurationChangeTokenSource);

            var namedConfigureFromConfigurationOptionsType =
                typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionsType);

            void BinderOptionsAction(BinderOptions options)
            {
                options.BindNonPublicProperties = attribute.BindNonPublicProperties;
                options.ErrorOnUnknownConfiguration = attribute.ErrorOnUnknownConfiguration;
            }

            var namedConfigureFromConfigurationOptions =
                Activator.CreateInstance(namedConfigureFromConfigurationOptionsType, name, section,
                    (Action<BinderOptions>)BinderOptionsAction);
            if (namedConfigureFromConfigurationOptions == null)
            {
                throw new ArgumentNullException(nameof(namedConfigureFromConfigurationOptions));
            }

            services.AddSingleton(typeof(IConfigureOptions<>).MakeGenericType(optionsType),
                _ => namedConfigureFromConfigurationOptions);
        }
    }
}
