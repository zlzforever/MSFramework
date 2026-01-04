using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MicroserviceFramework.Common;
using MicroserviceFramework.Extensions.Options;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

[assembly: InternalsVisibleTo("MSFramework.Serialization.Newtonsoft")]
[assembly: InternalsVisibleTo("MSFramework.AspNetCore")]

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builderAction"></param>
    /// <param name="scanAssemblyPrefixes">指定工具类 Utils.Runtime 中扫描程序集的前缀
    /// 整个框架对程序集的扫描，类型的发现都通过 RuntimeUtilities 来实现，即受此前缀影响</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void AddMicroserviceFramework(this IServiceCollection services,
        Action<MicroserviceFrameworkBuilder> builderAction = null, params string[] scanAssemblyPrefixes)
    {
        foreach (var prefix in scanAssemblyPrefixes)
        {
            Utils.Runtime.StartsWith.Add(prefix);
        }

        Utils.Runtime.Load();
        var builder = new MicroserviceFrameworkBuilder(services);
        builder.Services.TryAddSingleton<ApplicationInfo>();
        builderAction?.Invoke(builder);
        builder.UseTextJsonSerializer();
    }

    /// <summary>
    /// 通过 OptionsType 特性使类自动绑定为 Options
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseOptionsType(this MicroserviceFrameworkBuilder builder,
        IConfiguration configuration)
    {
        builder.Services.AddOptionsType(configuration);
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="applicationServices"></param>
    public static void UseMicroserviceFramework(this IServiceProvider applicationServices)
    {
        var defaultJsonSerializer = applicationServices.GetService<IJsonSerializer>();
        Defaults.JsonSerializer = defaultJsonSerializer ?? TextJsonSerializer.Create();

        var loggerFactory = applicationServices.GetService<ILoggerFactory>();
        if (loggerFactory == null)
        {
            Defaults.Logger = NullLogger.Instance;
        }
        else
        {
            var logger = loggerFactory.CreateLogger("MicroserviceFramework");
            Defaults.Logger = logger;
        }

        var initializers = applicationServices.GetServices<IInitializer>()
            .OrderByDescending(x => x.Order).ToList();

        if (initializers.Count > 0)
        {
            Defaults.Logger.LogInformation(
                "发现初始化器: {Initializers}",
                string.Join(" -> ", initializers.Select(x => x.GetType().FullName)));
            foreach (var initializer in initializers)
            {
                initializer.Start();
            }
        }
    }

    internal static void TryAdd(this IServiceCollection collection, ServiceDescriptor serviceDescriptor)
    {
        Check.NotNull(collection, nameof(collection));
        Check.NotNull(serviceDescriptor, nameof(serviceDescriptor));

        foreach (var x in collection)
        {
            if (x == null)
            {
                continue;
            }

            if (x.ServiceType == serviceDescriptor.ServiceType &&
                (
                    serviceDescriptor.ImplementationType != null &&
                    x.ImplementationType == serviceDescriptor.ImplementationType
                    || serviceDescriptor.ImplementationFactory != null &&
                    x.ImplementationFactory?.GetHashCode() ==
                    serviceDescriptor.ImplementationFactory.GetHashCode()
                    || serviceDescriptor.ImplementationInstance != null && x.ImplementationInstance ==
                    serviceDescriptor.ImplementationInstance
                ) &&
                x.Lifetime == serviceDescriptor.Lifetime)
            {
                return;
            }
        }

        collection.Add(serviceDescriptor);
    }
}
