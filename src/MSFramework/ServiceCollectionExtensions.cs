using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MicroserviceFramework.Common;
using MicroserviceFramework.Extensions.Options;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json;
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
/// MSFramework 框架服务注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 框架注册完成标志：非测试进程只允许成功完成一次注册（方法以
    /// <see cref="MethodImplOptions.Synchronized"/> 串行，读写无并发竞争）
    /// </summary>
    private static int _initialized;

    /// <summary>
    /// 注册 MSFramework 核心服务，初始化程序集扫描和框架构建器。
    /// 非测试进程只允许调用一次：重复调用会重复扫描注册（服务描述符翻倍、
    /// <c>IEnumerable&lt;T&gt;</c> 解析出双倍实例），且迟到的扫描前缀在
    /// <see cref="Utils.Runtime.Load"/> 幂等跳过后不会生效——二者均为静默错误，故直接抛异常；
    /// 多模块共享宿主时，请在首次调用传入全部扫描前缀。测试进程（<see cref="Defaults.IsInTests"/>）
    /// 需构建多个独立宿主，豁免该限制。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="builderAction">框架构建器配置委托</param>
    /// <param name="scanAssemblyPrefixes">指定工具类 Utils.Runtime 中扫描程序集的前缀
    /// 整个框架对程序集的扫描，类型的发现都通过 RuntimeUtilities 来实现，即受此前缀影响</param>
    /// <exception cref="InvalidOperationException">非测试进程重复调用时抛出</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void AddMicroserviceFramework(this IServiceCollection services,
        Action<MicroserviceFrameworkBuilder> builderAction = null, params string[] scanAssemblyPrefixes)
    {
        // 一次性守卫：标志在成功完成后置位，首次调用中途失败可重试；
        // 不把内部注册改为 TryAdd——"后注册覆盖前注册"是 DI 的正常覆盖语义（使用方可显式
        // 注册替换框架默认项），重复注册的保护由本守卫完成而非 TryAdd
        if (_initialized != 0 && !Defaults.IsInTests)
        {
            throw new InvalidOperationException(
                "AddMicroserviceFramework 只能调用一次；重复调用会重复扫描注册，且迟到的扫描前缀不会生效。多个模块共享宿主时，请在首次调用传入全部扫描前缀。");
        }

        foreach (var prefix in scanAssemblyPrefixes)
        {
            Utils.Runtime.StartsWith.Add(prefix);
        }

        Utils.Runtime.Load();
        var builder = new MicroserviceFrameworkBuilder(services);
        builder.Services.TryAddSingleton<ApplicationInfo>();
        builderAction?.Invoke(builder);
        builder.UseTextJsonSerializer();
        _initialized = 1;
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
    /// 初始化 MSFramework，设置默认序列化器、日志和初始化器。
    /// 本方法是全局默认配置（<see cref="Defaults"/>）的唯一初始化点，应在应用启动阶段调用一次：
    /// 非测试环境下重复调用（二次初始化全局默认项）会抛出 <see cref="InvalidOperationException"/>；
    /// 测试进程因 <see cref="Defaults.IsInTests"/> 豁免，可构建多个宿主。
    /// </summary>
    /// <param name="applicationServices">应用程序服务提供者</param>
    public static void UseMicroserviceFramework(this IServiceProvider applicationServices)
    {
        var defaultJsonSerializer = applicationServices.GetService<IJsonSerializer>();
        Defaults.JsonSerializer = defaultJsonSerializer ?? TextJsonSerializer.Create();
        Defaults.ServiceProvider = applicationServices;

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

        // 按 Order 降序执行：与 IInitializer/InitializerBase 注释契约一致（值越大越先执行）。
        // 采用降序是有意保持历史行为：EntityFrameworkInitializerBase 使用 int.MaxValue
        // 保证数据库迁移先于依赖表结构的业务初始化器（如数据播种）执行。
        // OrderByDescending 保持稳定排序，相同 Order 的初始化器仍按注册顺序执行
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

    // internal static void TryAdd(this IServiceCollection collection, ServiceDescriptor serviceDescriptor)
    // {
    //     Check.NotNull(collection, nameof(collection));
    //     Check.NotNull(serviceDescriptor, nameof(serviceDescriptor));
    //
    //     foreach (var x in collection)
    //     {
    //         if (x == null)
    //         {
    //             continue;
    //         }
    //
    //         if (x.ServiceType == serviceDescriptor.ServiceType &&
    //             (
    //                 serviceDescriptor.ImplementationType != null &&
    //                 x.ImplementationType == serviceDescriptor.ImplementationType
    //                 || serviceDescriptor.ImplementationFactory != null &&
    //                 x.ImplementationFactory?.GetHashCode() ==
    //                 serviceDescriptor.ImplementationFactory.GetHashCode()
    //                 || serviceDescriptor.ImplementationInstance != null && x.ImplementationInstance ==
    //                 serviceDescriptor.ImplementationInstance
    //             ) &&
    //             x.Lifetime == serviceDescriptor.Lifetime)
    //         {
    //             return;
    //         }
    //     }
    //
    //     collection.Add(serviceDescriptor);
    // }
}
