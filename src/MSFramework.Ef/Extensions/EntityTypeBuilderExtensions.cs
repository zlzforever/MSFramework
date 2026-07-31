using System;
using System.Linq.Expressions;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// 实体类型构建器扩展方法：
/// 1. 审计字段配置（已由 <c>EntityPropertyConventionStrategy</c> 自动处理，标记为过时）；
/// 2. 复合主键（值对象键）映射辅助 <see cref="ConfigureCompositeKey"/>。
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

    /// <summary>
    /// 将值对象主键映射为复合键（单列存储）。
    /// <para>
    /// 适用场景：聚合根的 <c>Id</c> 为复合主键值对象（如 <c>OrderItemKey(OrderId, ProductId)</c>）。
    /// EF Core 10 及以下不支持复杂类型（ComplexProperty）/ owned 类型（OwnsOne）成员作为主键
    /// （该能力自 EF Core 11 起提供），本框架在 EF Core 10 下采用 <see cref="ValueConverter{TModel,TProvider}"/>
    /// 将值对象序列化为单列主键，从而保持 <see cref="IRepository{TEntity,TKey}"/> 体系零改动，
    /// <see cref="DbSet{TEntity}.Find(object[])"/> 与主键表达式查询均可直接使用值对象。
    /// </para>
    /// <para>
    /// 列名默认使用主键属性名（如 <c>Id</c>），由 <c>EntityPropertyConventionStrategy</c>
    /// 按 snake_case 约定统一转换为小写下划线（如 <c>id</c>）；如需自定义列名可传入 <paramref name="columnName"/>，
    /// 该列名同样会被 snake_case 约定处理（与框架其余列行为一致）。
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">实体类型，需实现 <see cref="IEntity{TKey}"/></typeparam>
    /// <typeparam name="TKey">复合主键值对象类型</typeparam>
    /// <param name="builder">实体类型构建器</param>
    /// <param name="keyExpression">主键属性访问表达式（如 <c>x =&gt; x.Id</c>）</param>
    /// <param name="toDatabaseValue">值对象到数据库字符串的转换委托（如 <c>v =&gt; v.ToString()</c>）</param>
    /// <param name="fromDatabaseValue">数据库字符串到值对象的转换委托（如 <c>s =&gt; OrderItemKey.Parse(s)</c>）</param>
    /// <param name="columnName">主键列名，为空时使用主键属性名（由 snake_case 约定转换）</param>
    /// <returns>实体类型构建器，便于链式调用</returns>
    /// <exception cref="ArgumentNullException">keyExpression / toDatabaseValue / fromDatabaseValue 为空时抛出</exception>
    public static EntityTypeBuilder<TEntity> ConfigureCompositeKey<TEntity, TKey>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TKey>> keyExpression,
        Func<TKey, string> toDatabaseValue,
        Func<string, TKey> fromDatabaseValue,
        string? columnName = null)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(toDatabaseValue);
        ArgumentNullException.ThrowIfNull(fromDatabaseValue);

        var propertyBuilder = builder.Property(keyExpression);
        propertyBuilder.HasConversion(new ValueConverter<TKey, string>(v => toDatabaseValue(v), s => fromDatabaseValue(s)));
        if (!string.IsNullOrWhiteSpace(columnName))
        {
            propertyBuilder.HasColumnName(columnName);
        }

        var keySelector = Expression.Lambda<Func<TEntity, object>>(
            Expression.Convert(keyExpression.Body, typeof(object)),
            keyExpression.Parameters);
        builder.HasKey(keySelector);
        return builder;
    }
}
