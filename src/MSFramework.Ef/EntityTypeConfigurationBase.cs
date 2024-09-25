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
public abstract class EntityTypeConfigurationBase<TEntity, TDbContext> :
    IEntityTypeConfiguration<TEntity, TDbContext>
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="modelBuilder"></param>
    public void Configure(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<TEntity>();
        Configure(builder);
    }

    /// <summary>
    /// 重写以实现实体类型各个属性的数据库配置
    /// </summary>
    /// <param name="builder">实体类型创建器</param>
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);

    /// <summary>
    ///
    /// </summary>
    /// <param name="_"></param>
    protected void ConfigureDefaultIdentifier(EntityTypeBuilder<TEntity> _)
    {
        // var propertyBuilder = builder.Property("Id");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"DbContext: {typeof(TDbContext)}, EntityType: {typeof(TEntity)}";
    }
}
