using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MicroserviceFramework.Common;
using MicroserviceFramework.Extensions.Options;
using MicroserviceFramework.Serialization;
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

/// <summary>
///
/// </summary>
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builderAction"></param>
    public static void AddMicroserviceFramework(this IServiceCollection services,
        Action<MicroserviceFrameworkBuilder> builderAction = null)
    {
        var builder = new MicroserviceFrameworkBuilder(services);
        builder.Services.TryAddSingleton<ApplicationInfo>();
        // 放到后面，加载优先级更高
        builderAction?.Invoke(builder);

        builder.UseTextJsonSerializer();

        // // 保证这在最后， 不然类型扫描事件的注册会晚于扫描
        // MicroserviceFrameworkLoaderContext.Get(services).LoadTypes();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="applicationServices"></param>
    public static void UseMicroserviceFramework(this IServiceProvider applicationServices)
    {
        var defaultJsonSerializer = applicationServices.GetService<IJsonSerializer>();
        Defaults.JsonSerializer = defaultJsonSerializer ?? TextJsonSerializer.Create();

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

        var initializers = applicationServices.GetServices<IInitializerBase>()
            .OrderBy(x => x.Order).ToList();
        logger.LogInformation(
            "发现初始化器: {Initializers}", string.Join(" -> ", initializers.Select(x => x.GetType().FullName)));

        CancellationToken cancellationToken;
        var hostApplicationLifetime = applicationServices.GetService<IHostApplicationLifetime>();
        if (hostApplicationLifetime != null)
        {
            using var combinedCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(hostApplicationLifetime.ApplicationStopping);
            cancellationToken = combinedCancellationTokenSource.Token;
            cancellationToken.ThrowIfCancellationRequested();
        }
        else
        {
            cancellationToken = CancellationToken.None;
        }

        foreach (var hostedService in initializers)
        {
            hostedService.StartAsync(cancellationToken).ConfigureAwait(false).GetAwaiter();
        }
    }

    /// <summary>
    /// 注意：此方法必须第一个调用
    /// </summary>
    /// <param name="services"></param>
    /// <param name="prefixes"></param>
    /// <returns></returns>
    public static IServiceCollection AddAssemblyScanPrefix(this IServiceCollection services,
        params string[] prefixes)
    {
        Check.NotNull(prefixes, nameof(prefixes));

        foreach (var prefix in prefixes)
        {
            Utils.Runtime.StartsWith.Add(prefix);
        }

        return services;
    }

    /// <summary>
    /// 指定工具类 RuntimeUtilities 中扫描程序集的前缀
    /// 整个框架对程序集的扫描，类型的发现都通过 RuntimeUtilities 来实现，即受此前缀影响
    /// 注意：此方法必须第一个调用
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="prefixes"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseAssemblyScanPrefix(this MicroserviceFrameworkBuilder builder,
        params string[] prefixes)
    {
        builder.Services.AddAssemblyScanPrefix(prefixes);
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
