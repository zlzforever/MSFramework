using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MicroserviceFramework.Common;
using MicroserviceFramework.Extensions.Options;
using MicroserviceFramework.Text.Json;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("MSFramework.Serialization.Newtonsoft")]
[assembly: InternalsVisibleTo("MSFramework.AspNetCore")]

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework;

public static class ServiceCollectionExtensions
{
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

    public static void AddMicroserviceFramework(this IServiceCollection services,
        Action<MicroserviceFrameworkBuilder> builderAction = null)
    {
        var builder = new MicroserviceFrameworkBuilder(services);

        builder.Services.TryAddSingleton<ApplicationInfo>();
        builder.UseDefaultJsonHelper();

        // 放到后面，加载优先级更高
        builderAction?.Invoke(builder);

        // 请保证这在最后，不然类型扫描事件的注册会晚于扫描
        MicroserviceFrameworkLoaderContext.Get(services).LoadTypes();
    }

    public static void UseMicroserviceFramework(this IServiceProvider applicationServices)
    {
        var configuration = applicationServices.GetService<IConfiguration>();
        if (configuration == null)
        {
            return;
        }

        var loggerFactory = applicationServices.GetService<ILoggerFactory>();
        if (loggerFactory == null)
        {
            return;
        }

        var logger = loggerFactory.CreateLogger("MicroserviceFramework");

        var initializers = applicationServices.GetServices<IHostedService>().Where(x => x is InitializerBase)
            .ToList();
        logger.LogInformation(
            "初始化器: {Initializers}", string.Join(" -> ", initializers.Select(x => x.GetType().FullName)));
    }

    /// <summary>
    /// 指定工具类 RuntimeUtilities 中扫描程序集的前缀
    /// 整个框架对程序集的扫描， 类型的发现都通过 RuntimeUtilities 来实现， 即受此前缀影响
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="prefixes"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseAssemblyScanPrefix(this MicroserviceFrameworkBuilder builder,
        params string[] prefixes)
    {
        Check.NotNull(prefixes, nameof(prefixes));

        foreach (var prefix in prefixes)
        {
            Utils.Runtime.StartsWith.Add(prefix);
        }

        return builder;
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
