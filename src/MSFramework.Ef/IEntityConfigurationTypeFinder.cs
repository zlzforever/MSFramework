using System;
using System.Collections.Generic;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 实体配置查找器，全局单例。按 DbContext 类型分组返回已注册的实体配置。
/// </summary>
public interface IEntityConfigurationTypeFinder
{
    /// <summary>
    /// 获取指定 DbContext 下已注册的实体配置。
    /// </summary>
    IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType);

    /// <summary>
    /// 获取指定实体所属的 DbContext 类型。
    /// </summary>
    Type GetDbContextTypeForEntity(Type entityType);

    /// <summary>
    /// 获取所有已注册的 DbContext 类型。
    /// </summary>
    IEnumerable<Type> GetAllDbContextTypes();

    /// <summary>
    /// 判断指定实体类型是否有注册到 DbContext。
    /// </summary>
    bool HasDbContextForEntity<T>();
}
