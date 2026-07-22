using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.AutoMapper;

/// <summary>
///     AutoMapper 对象映射器的依赖注入扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     通过 builder 方式注册 AutoMapper 对象映射器（扫描所有程序集）
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddAutoMapperObjectAssembler();
        return builder;
    }

    /// <summary>
    ///     注册 AutoMapper 对象映射器到服务集合（扫描所有程序集）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAutoMapperObjectAssembler(this IServiceCollection services)
    {
        services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        services.AddAutoMapper(_ =>
        {
        }, Utils.Runtime.GetAllAssemblies());
        return services;
    }

    /// <summary>
    ///     通过 builder 方式注册 AutoMapper 对象映射器（指定程序集列表）
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <param name="assemblies">要扫描的程序集列表</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder,
        params Assembly[] assemblies)
    {
        builder.Services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        builder.Services.AddAutoMapper(_ =>
        {
        }, assemblies);
        return builder;
    }
}
