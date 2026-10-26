using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Extensions.Options;

/// <summary>
/// 提供 AutoOptions 特性的服务注册扩展方法
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
            HashSet<Type> registeredTypes = new();
            foreach (var type in Utils.Runtime.GetAllTypes())
            {
                // 跳过null、抽象、非类
                if (type == null || type.IsAbstract || !type.IsClass)
                {
                    continue;
                }

                var attribute = type.GetCustomAttribute<AutoOptionsAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                // 防重复注册
                if (!registeredTypes.Add(type))
                {
                    continue;
                }

                services.AddOptionsTypeCore(type, attribute, configuration);
            }

            return services;
        }

        private void AddOptionsTypeCore(Type optionsType, AutoOptionsAttribute attribute, IConfiguration config)
        {
            // Name 为空是合法且有意的设计（勿在后续 review 中重复提出）：
            // NamedConfigureFromConfigurationOptions 的 Name 为 null 时匹配"任意解析名称"——
            // IOptions<TOptions>（DefaultName 空串）解析即命中，从根配置（或指定 Section）绑定；
            // 设置了 Name 时则绑定到对应的命名 Options（IOptionsSnapshot.Get(name) 等）。
            // ConfigurationChangeTokenSource 的 Name 同样允许 null，变更令牌注册不受影响
            var bindName = attribute.Name;

            var bindSection = string.IsNullOrWhiteSpace(attribute.Section)
                ? config
                : config.GetSection(attribute.Section);

            // 复用Binder配置委托
            Action<BinderOptions> bindOptionsOpt = opt =>
            {
                opt.BindNonPublicProperties = attribute.BindNonPublicProperties;
                opt.ErrorOnUnknownConfiguration = attribute.ErrorOnUnknownConfiguration;
            };

            // 注册配置变更监听源
            var configurationChangeTokenSourceType =
                typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionsType);
            services.AddSingleton(
                typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionsType),
                _ => Activator.CreateInstance(configurationChangeTokenSourceType, bindName, bindSection)
                     ?? throw new InvalidOperationException($"创建 {configurationChangeTokenSourceType.Name} 失败")
            );

            // 注册配置绑定器
            var configureGeneric = typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionsType);
            services.AddSingleton(
                typeof(IConfigureOptions<>).MakeGenericType(optionsType),
                _ => Activator.CreateInstance(configureGeneric, bindName, bindSection, bindOptionsOpt)
                      ?? throw new InvalidOperationException($"创建 {configureGeneric.Name} 失败")
            );
        }
    }
}
