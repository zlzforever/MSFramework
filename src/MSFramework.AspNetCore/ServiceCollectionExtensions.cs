using System.IO;
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
    public static IServiceCollection AddAspNetCoreExtension(this IServiceCollection services)
    {
        services.TryAddScoped<ISession>(provider =>
            HttpSession.Create(provider.GetRequiredService<IHttpContextAccessor>()));
        services.TryAddSingleton(x =>
            x.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        if (!Directory.Exists(Defaults.LocalOSSDirectory))
        {
            Directory.CreateDirectory(Defaults.LocalOSSDirectory);
        }

        return services;
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
