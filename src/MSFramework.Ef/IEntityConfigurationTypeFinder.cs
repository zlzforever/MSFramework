using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 定义实体类配置类型查找器
/// </summary>
public interface IEntityConfigurationTypeFinder
{
    /// <summary>
    /// 获取指定上下文类型的实体配置注册信息
    /// </summary>
    /// <param name="dbContextType">数据上下文类型</param>
    /// <returns></returns>
    IEnumerable<EntityTypeConfigurationMetadata> GetEntityTypeConfigurations(Type dbContextType);

    /// <summary>
    /// 获取 实体类所属的数据上下文类
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <returns>数据上下文类型</returns>
    Type GetDbContextTypeForEntity(Type entityType);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    IEnumerable<Type> GetAllDbContextTypes();

    /// <summary>
    /// 判断实体有无配置到 DbContext
    /// </summary>
    /// <returns></returns>
    bool HasDbContextForEntity<T>();
}

/// <summary>
///
/// </summary>
/// <param name="entityType"></param>
/// <param name="configureMethodInfo"></param>
/// <param name="createEntityTypeBuilderMethod"></param>
/// <param name="entityTypeConfiguration"></param>
public struct EntityTypeConfigurationMetadata(
    Type entityType,
    MethodInfo configureMethodInfo,
    MethodInfo createEntityTypeBuilderMethod,
    IEntityTypeConfiguration entityTypeConfiguration)
{
    /// <summary>
    ///
    /// </summary>
    public readonly Type EntityType = entityType;
    /// <summary>
    ///
    /// </summary>
    public readonly MethodInfo ConfigureMethodInfo = configureMethodInfo;
    /// <summary>
    ///
    /// </summary>
    public readonly MethodInfo CreateEntityTypeBuilderMethod = createEntityTypeBuilderMethod;
    /// <summary>
    ///
    /// </summary>
    public readonly IEntityTypeConfiguration EntityTypeConfiguration = entityTypeConfiguration;
}
