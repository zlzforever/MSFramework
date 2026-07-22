using System;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef;

// public abstract class EntityTypeConfigurationBase<TEntity> : EntityTypeConfigurationBase<TEntity, DefaultDbContext>
//     where TEntity : class, IEntity;

/// <summary>
/// 数据实体映射配置基类
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TDbContext"></typeparam>
public abstract class EntityTypeConfigurationBase<TEntity, TDbContext> : IEntityTypeConfiguration<TEntity>,
    IEntityTypeConfiguration
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    /// <summary>
    /// 实现 IEntityTypeConfiguration.Configure，创建实体构建器后执行具体配置
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    public void Configure(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<TEntity>();
        Configure(builder);

        if (typeof(TEntity).IsAssignableTo(Defaults.Types.ExternalEntity))
        {
            builder.Metadata.SetIsTableExcludedFromMigrations(true);
        }
    }

    /// <summary>
    /// 获取实体类型
    /// </summary>
    public Type GetEntityType() => typeof(TEntity);

    /// <summary>
    /// 获取所属 DbContext 类型
    /// </summary>
    public Type GetDbContextType() => typeof(TDbContext);

    /// <summary>
    /// 重写以实现实体类型各个属性的数据库配置
    /// </summary>
    /// <param name="builder">实体类型创建器</param>
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);

    /// <summary>
    /// 配置默认主键（预留扩展点，当前未实现）
    /// </summary>
    /// <param name="_">实体类型构建器（未使用）</param>
    protected void ConfigureDefaultIdentifier(EntityTypeBuilder<TEntity> _)
    {
        // var propertyBuilder = builder.Property("Id");
    }

    /// <summary>
    /// 返回配置的描述信息
    /// </summary>
    /// <returns>描述字符串</returns>
    public override string ToString()
    {
        return $"DbContext: {typeof(TDbContext)}, EntityType: {typeof(TEntity)}";
    }
}
