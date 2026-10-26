using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper;
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
    /// <param name="configAction">AutoMapper 配置委托，可为 null（使用默认配置）</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder,
        Action<IMapperConfigurationExpression> configAction = null)
    {
        RegisterAutoMapperObjectAssembler(builder.Services, configAction, Utils.Runtime.GetAllAssemblies());
        return builder;
    }

    /// <summary>
    ///     通过 builder 方式注册 AutoMapper 对象映射器（扫描指定程序集，使用默认配置）。
    ///     兼容旧 API 形态 <c>UseAutoMapperObjectAssembler(typeof(Profile).Assembly)</c>，
    ///     避免旧调用因新增 configAction 重载后无法编译
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <param name="assemblies">要扫描的程序集列表</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder,
        params Assembly[] assemblies)
    {
        RegisterAutoMapperObjectAssembler(builder.Services, null, assemblies);
        return builder;
    }

    /// <summary>
    ///     注册 AutoMapper 对象映射器到服务集合（扫描所有程序集）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configAction">AutoMapper 配置委托，可为 null（使用默认配置）</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAutoMapperObjectAssembler(this IServiceCollection services,
        Action<IMapperConfigurationExpression> configAction = null)
    {
        RegisterAutoMapperObjectAssembler(services, configAction, Utils.Runtime.GetAllAssemblies());
        return services;
    }

    /// <summary>
    ///     注册 AutoMapper 对象映射器到服务集合（扫描指定程序集，使用默认配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assemblies">要扫描的程序集列表</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAutoMapperObjectAssembler(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        RegisterAutoMapperObjectAssembler(services, null, assemblies);
        return services;
    }

    /// <summary>
    ///     通过 builder 方式注册 AutoMapper 对象映射器（指定程序集列表）
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <param name="configAction">AutoMapper 配置委托，此重载要求显式传入（不与程序集参数互换位置）</param>
    /// <param name="assemblies">要扫描的程序集列表</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder,
        Action<IMapperConfigurationExpression> configAction,
        params Assembly[] assemblies)
    {
        RegisterAutoMapperObjectAssembler(builder.Services, configAction, assemblies);
        return builder;
    }

    /// <summary>
    ///     注册 AutoMapper 对象映射器核心逻辑：注册 <see cref="IObjectAssembler"/> 并配置 AutoMapper 程序集扫描。
    ///     各公共重载统一委托本私有方法，避免公共重载之间相互转发产生重载解析歧义
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configAction">AutoMapper 配置委托，可为 null（使用默认配置）</param>
    /// <param name="assemblies">要扫描的程序集列表</param>
    private static void RegisterAutoMapperObjectAssembler(IServiceCollection services,
        Action<IMapperConfigurationExpression> configAction, IEnumerable<Assembly> assemblies)
    {
        services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        services.AddAutoMapper(configAction, assemblies);
    }
}
