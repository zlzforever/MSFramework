using System;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// 审计字段扩展方法。已由 <c>EntityPropertyConventionStrategy</c> 自动处理，
/// 实现 ICreation/IModification/IDeletion 的实体会自动获得默认配置。
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// 设置所有审计字段，已由框架自动配置。
    /// </summary>
    [Obsolete("审计字段已由 EntityPropertyConventionStrategy 自动配置，可安全移除此调用。")]
    public static void ConfigureAuditProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ICreation, IModification, IDeletion
    {
        builder.ConfigureCreation();
        builder.ConfigureModification();
        builder.ConfigureDeletion();
    }

    /// <summary>
    /// 设置创建审计字段，已由框架自动配置。
    /// </summary>
    [Obsolete("创建审计字段已由 EntityPropertyConventionStrategy 自动配置，可安全移除此调用。")]
    public static void ConfigureCreation<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ICreation
    {
        builder.Property(x => x.CreationTime).UseUnixTime();
        builder.Property(x => x.CreatorId).HasMaxLength(36);
        builder.Property(x => x.CreatorName).HasMaxLength(256);
    }

    /// <summary>
    /// 设置修改审计字段，已由框架自动配置。
    /// </summary>
    [Obsolete("修改审计字段已由 EntityPropertyConventionStrategy 自动配置，可安全移除此调用。")]
    public static void ConfigureModification<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IModification
    {
        builder.Property(x => x.LastModificationTime).UseUnixTime();
        builder.Property(x => x.LastModifierId).HasMaxLength(36);
        builder.Property(x => x.LastModifierName).HasMaxLength(256);
    }

    /// <summary>
    /// 设置删除审计字段，已由框架自动配置。
    /// </summary>
    [Obsolete("删除审计字段已由 EntityPropertyConventionStrategy 自动配置，可安全移除此调用。")]
    public static void ConfigureDeletion<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IDeletion
    {
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletionTime).UseUnixTime();
        builder.Property(x => x.DeleterId).HasMaxLength(36);
        builder.Property(x => x.DeleterName).HasMaxLength(256);
    }
}
