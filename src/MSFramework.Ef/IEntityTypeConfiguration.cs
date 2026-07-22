using System;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 实体类型配置的非泛型接口，用于框架内部反射调用
/// </summary>
public interface IEntityTypeConfiguration
{
    /// <summary>
    /// 执行实体类型配置
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    void Configure(ModelBuilder modelBuilder);

    /// <summary>
    /// 获取实体类型
    /// </summary>
    /// <returns>实体 CLR 类型</returns>
    Type GetEntityType();

    /// <summary>
    /// 获取所属 DbContext 类型
    /// </summary>
    /// <returns>DbContext CLR 类型</returns>
    Type GetDbContextType();
}
