using System;
using System.IO;
using System.Text.Json;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///     ASP.NET Core 的依赖注入扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     注册 <see cref="HttpContextScopeServiceProvider" /> 为 <see cref="IScopeServiceProvider" /> 的单例实现
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddScopeServiceProvider(this IServiceCollection services)
    {
        services.AddSingleton<IScopeServiceProvider, HttpContextScopeServiceProvider>();
        return services;
    }

    /// <summary>
    ///     通过 builder 方式启用 ScopeServiceProvider
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseScopeServiceProvider(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddScopeServiceProvider();
        return builder;
    }

    /// <summary>
    ///     注册 ASP.NET Core 集成所需的基础服务，包括 HttpSession、JsonSerializerOptions 以及本地 OSS 目录
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    /// <exception cref="MicroserviceFrameworkException">
    ///     本地 OSS 目录创建失败（权限不足、只读文件系统）时在启动期立即抛出
    /// </exception>
    /// <remarks>
    ///     本地 OSS 目录在<b>启动期（注册阶段）即创建</b>——设计决策：文件系统权限问题
    ///     （只读容器、目录权限不足）必须在部署时就暴露出来，让运维在上线前调整权限/挂载卷；
    ///     若延迟到首次保存文件才失败，就是"系统上线一段时间后报异常"的被动局面。
    ///     失败时抛出带修复指引的明确错误（见 <see cref="EnsureLocalOssDirectory"/>）
    /// </remarks>
    public static IServiceCollection AddAspNetCoreExtension(this IServiceCollection services)
    {
        services.TryAddScoped<ISession>(provider =>
            HttpSession.Create(provider.GetRequiredService<IHttpContextAccessor>()));
        // JsonOptions 依赖 AddControllers() 先行注册（IOptions<JsonOptions> 由 MVC 注册）；
        // 隐式依赖在此显式化：缺失时抛出带根因的明确错误，而非延迟到首次解析时
        // 报语义不明的 "No service for type 'IOptions<JsonOptions>'"
        services.TryAddSingleton(x => ResolveJsonSerializerOptions(x));

        EnsureLocalOssDirectory();

        return services;
    }

    /// <summary>
    ///     启动期创建本地 OSS 存储目录；失败时抛出含根因与修复指引的明确错误，
    ///     而非裸 <see cref="System.UnauthorizedAccessException"/>（原始异常不含路径配置信息，难以定位）
    /// </summary>
    /// <exception cref="MicroserviceFrameworkException">目录创建失败（权限不足、只读文件系统等）时抛出</exception>
    private static void EnsureLocalOssDirectory()
    {
        if (Directory.Exists(Defaults.LocalOSSDirectory))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(Defaults.LocalOSSDirectory);
        }
        catch (System.Exception e)
        {
            throw new MicroserviceFrameworkException(
                $"本地 OSS 存储目录创建失败：{Defaults.LocalOSSDirectory}（应用根下 storage/oss，路径不可配置）。" +
                "常见原因：容器文件系统只读、目录权限不足。" +
                "请在部署时挂载可写卷或调整目录权限", e);
        }
    }

    /// <summary>
    ///     解析 ASP.NET Core 的全局 JSON 序列化选项（<c>IOptions&lt;JsonOptions&gt;</c>）。
    ///     未找到（宿主未调用 <c>AddControllers()</c>）时抛出带根因与修复指引的明确错误，
    ///     而非延迟到首次解析时报语义不明的 "No service for type 'IOptions&lt;JsonOptions&gt;'"
    /// </summary>
    /// <param name="serviceProvider">服务提供程序</param>
    /// <returns>全局 JSON 序列化选项</returns>
    /// <exception cref="MicroserviceFrameworkException">未注册 <c>IOptions&lt;JsonOptions&gt;</c>（未调用 AddControllers）时抛出</exception>
    private static JsonSerializerOptions ResolveJsonSerializerOptions(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<IOptions<JsonOptions>>() is { } jsonOptions
            ? jsonOptions.Value.JsonSerializerOptions
            : throw new MicroserviceFrameworkException(
                "未找到 ASP.NET Core 的 JsonOptions（IOptions<JsonOptions> 未注册）。" +
                "请在调用 UseAspNetCoreExtension 之前先执行 AddControllers()（MVC 负责注册 JsonOptions）");
    }

    /// <summary>
    ///     通过 builder 方式启用 ASP.NET Core 扩展
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseAspNetCoreExtension(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddAspNetCoreExtension();
        return builder;
    }

    /// <summary>
    ///     初始化 MSFramework 中间件管道
    /// </summary>
    /// <param name="builder">应用构建器</param>
    /// <returns>应用构建器</returns>
    public static IApplicationBuilder UseMicroserviceFramework(this IApplicationBuilder builder)
    {
        builder.ApplicationServices.UseMicroserviceFramework();
        return builder;
    }

    /// <summary>
    ///     配置模型验证失败的响应格式，使用 <see cref="InvalidModelStateResponseFactory" /> 统一输出
    /// </summary>
    /// <param name="builder">MVC 构建器</param>
    /// <returns>MVC 构建器</returns>
    public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(x =>
        {
            x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
        });
        return builder;
    }
}
