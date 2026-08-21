using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Internal;

// ============================================================================
// 接口
// ============================================================================

/// <summary>
/// 模型构建策略，封装 OnModelCreating 中的单一配置关注点。
/// 所有实现类均为无状态 internal sealed，通过 <see cref="DbContextBase"/> 的静态管道数组执行。
/// </summary>
internal interface IModelBuildingStrategy
{
    void Apply(ModelBuildingContext context);
}

// ============================================================================
// 上下文
// ============================================================================

/// <summary>
/// 传递给每个 <see cref="IModelBuildingStrategy"/> 的上下文，包含构建所需的所有依赖。
/// </summary>
internal record ModelBuildingContext(
    ModelBuilder ModelBuilder,
    DbContextSettings Settings,
    IEntityConfigurationTypeFinder ConfigFinder,
    Type DbContextType);

// ============================================================================
// 策略 1：注册实体类型配置
// ============================================================================

/// <summary>
/// 通过 <see cref="IEntityConfigurationTypeFinder"/> 查找当前 DbContext 的实体配置类，
/// 并逐一注册到 <see cref="ModelBuilder"/>。
/// </summary>
internal sealed class RegisterEntityConfigurationsStrategy : IModelBuildingStrategy
{
    public void Apply(ModelBuildingContext context)
    {
        var configurations = context.ConfigFinder.GetEntityTypeConfigurations(context.DbContextType);
        foreach (var config in configurations)
        {
            config.Configure(context.ModelBuilder);
        }
    }
}

// ============================================================================
// 策略 2：实体表级配置（表名重命名 + 软删除过滤器）
// ============================================================================

/// <summary>
/// 遍历所有非 Owned、非 Keyless 实体类型，应用表名规则（SnakeCase / 前缀）和软删除全局过滤器。
/// Keyless 实体（<c>HasNoKey</c>/<c>ToQuery</c> 只读查询模型）无表名映射，跳过表级约定。
/// </summary>
internal sealed class EntityTableConventionStrategy : IModelBuildingStrategy
{
    public void Apply(ModelBuildingContext context)
    {
        foreach (var entityType in context.ModelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned() || entityType.IsKeyless)
            {
                continue;
            }

            ApplyTableName(entityType, context.Settings);
            ApplySoftDeleteFilter(entityType, context.Settings);
        }
    }

    private static void ApplyTableName(IMutableEntityType entityType, DbContextSettings settings)
    {
        var defaultTableName = entityType.GetDefaultTableName();
        var tableName = entityType.GetTableName();

        // 仅在用户未设置自定义表名时自动调整
        if (defaultTableName != tableName)
        {
            return;
        }

        // 无表名映射（Keyless / ToView 实体）时跳过，避免对 null 调用 ToSnakeCase 抛 NRE
        if (string.IsNullOrEmpty(tableName))
        {
            return;
        }

        if (settings.UseUnderScoreCase)
        {
            tableName = tableName.ToSnakeCase();
        }

        if (!string.IsNullOrEmpty(settings.TablePrefix))
        {
            tableName = settings.TablePrefix + tableName;
        }

        entityType.SetTableName(tableName);
    }

    private static void ApplySoftDeleteFilter(IMutableEntityType entityType, DbContextSettings settings)
    {
        if (settings.UseCompiledModel)
        {
            return;
        }

        if (!typeof(IDeletion).IsAssignableFrom(entityType.ClrType))
        {
            return;
        }

        entityType.AddSoftDeleteQueryFilter();
    }
}

// ============================================================================
// 策略 3：实体属性级配置（列名 + 列类型 + 乐观锁）
// ============================================================================

/// <summary>
/// 遍历所有非 Owned、非 Keyless 实体类型的所有属性，应用：
///   1. 乐观锁（ConcurrencyStamp 配置为 ConcurrencyToken）
///   2. 列名转换（SnakeCase）
///   3. 列类型自动设置（ObjectId / Guid / Enumeration 的 ValueConverter 和 ColumnType）
/// Keyless 实体（<c>HasNoKey</c>/<c>ToQuery</c> 只读查询模型）无表名映射，
/// 其列名/列类型由查询定义自身决定，跳过属性级约定。
/// </summary>
internal sealed class EntityPropertyConventionStrategy : IModelBuildingStrategy
{
    public void Apply(ModelBuildingContext context)
    {
        var enumerationConverterType = typeof(EnumerationToStringConverter<>);

        foreach (var entityType in context.ModelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned() || entityType.IsKeyless)
            {
                continue;
            }

            var hasOptimisticLock = Defaults.Types.OptimisticLock.IsAssignableFrom(entityType.ClrType);
            var hasCreation = typeof(ICreation).IsAssignableFrom(entityType.ClrType);
            var hasModification = typeof(IModification).IsAssignableFrom(entityType.ClrType);
            var hasDeletion = typeof(IDeletion).IsAssignableFrom(entityType.ClrType);

            foreach (var property in entityType.GetProperties())
            {
                ApplyOptimisticLock(property, hasOptimisticLock);

                if (hasCreation)
                {
                    ApplyCreationDefaults(property);
                }

                if (hasModification)
                {
                    ApplyModificationDefaults(property);
                }

                if (hasDeletion)
                {
                    ApplyDeletionDefaults(property);
                }

                ApplyColumnName(property, entityType, context.Settings);
                ApplyColumnType(property, enumerationConverterType);
            }
        }
    }

    private static void ApplyOptimisticLock(IMutableProperty property, bool hasOptimisticLock)
    {
        if (!hasOptimisticLock || property.Name != nameof(IOptimisticLock.ConcurrencyStamp))
        {
            return;
        }

        if (property.GetMaxLength() == null)
        {
            property.SetMaxLength(36);
        }

        property.IsConcurrencyToken = true;
        property.IsNullable = true;
    }

    private static void ApplyColumnName(IMutableProperty property, IMutableEntityType entityType,
        DbContextSettings settings)
    {
        if (!settings.UseUnderScoreCase)
        {
            return;
        }

        var tableName = entityType.GetTableName();
        if (string.IsNullOrEmpty(tableName))
        {
            // Keyless / ToView 实体无表名映射，跳过列名约定（由查询/视图定义自身决定列名）
            return;
        }

        var storeObjectIdentifier =
            StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
        var columnName = property.GetColumnName(storeObjectIdentifier);

        if (string.IsNullOrEmpty(columnName))
        {
            throw new ArgumentException(
                $"Entity {entityType.Name} Property {property.Name} without column name");
        }

        if (columnName.StartsWith('_'))
        {
            columnName = columnName.Substring(1, columnName.Length - 1);
        }

        property.SetColumnName(columnName.ToSnakeCase());
    }

    private static void ApplyColumnType(IMutableProperty property, Type enumerationConverterType)
    {
        if (property.ClrType == Defaults.Types.ObjectId)
        {
            ApplyObjectIdColumnType(property);
        }
        else if (property.ClrType == Defaults.Types.Guid)
        {
            ApplyGuidColumnType(property);
        }
        else if (property.ClrType.IsAssignableTo(typeof(Enumeration)))
        {
            ApplyEnumerationColumnType(property, enumerationConverterType);
        }
    }

    private static void ApplyObjectIdColumnType(IMutableProperty property)
    {
        property.SetValueConverter(new ObjectIdToStringConverter());

        var maxLength = property.GetMaxLength();
        switch (maxLength)
        {
            case null:
                property.SetMaxLength(36);
                break;
            case < 24:
                throw new ArgumentException("ObjectId 长度不能小于 24");
        }

        var columnType = property.GetColumnType();
        if (string.IsNullOrEmpty(columnType))
        {
            property.SetColumnType("varchar");
        }

        property.ValueGenerated = ValueGenerated.Never;
    }

    private static void ApplyGuidColumnType(IMutableProperty property)
    {
        var maxLength = property.GetMaxLength();
        switch (maxLength)
        {
            case null:
                property.SetMaxLength(36);
                break;
            case < 36:
                throw new ArgumentException("Guid 长度不能小于 36");
        }

        var columnType = property.GetColumnType();
        if (string.IsNullOrEmpty(columnType))
        {
            property.SetColumnType("varchar");
        }
    }

    private static void ApplyEnumerationColumnType(IMutableProperty property, Type enumerationConverterType)
    {
        var columnType = property.GetColumnType();
        if (string.IsNullOrEmpty(columnType))
        {
            property.SetColumnType("varchar");
        }

        if (property.GetMaxLength() == null)
        {
            property.SetMaxLength(256);
        }

        var converterType = enumerationConverterType.MakeGenericType(property.ClrType);
        var converter = (ValueConverter)Activator.CreateInstance(converterType);
        property.SetValueConverter(converter);
    }

    private static void ApplyCreationDefaults(IMutableProperty property)
    {
        switch (property.Name)
        {
            case nameof(ICreation.CreationTime):
                if (property.GetValueConverter() == null)
                {
                    SetUnixTimeDefaults(property);
                }

                break;
            case nameof(ICreation.CreatorId):
                if (property.GetMaxLength() == null)
                {
                    property.SetMaxLength(36);
                }

                break;
            case nameof(ICreation.CreatorName):
                if (property.GetMaxLength() == null)
                {
                    property.SetMaxLength(256);
                }

                break;
        }
    }

    private static void ApplyModificationDefaults(IMutableProperty property)
    {
        switch (property.Name)
        {
            case nameof(IModification.LastModificationTime):
                if (property.GetValueConverter() == null)
                {
                    SetUnixTimeDefaults(property);
                }

                break;
            case nameof(IModification.LastModifierId):
                if (property.GetMaxLength() == null)
                {
                    property.SetMaxLength(36);
                }

                break;
            case nameof(IModification.LastModifierName):
                if (property.GetMaxLength() == null)
                {
                    property.SetMaxLength(256);
                }

                break;
        }
    }

    private static void ApplyDeletionDefaults(IMutableProperty property)
    {
        switch (property.Name)
        {
            case nameof(IDeletion.IsDeleted):
                if (property.GetDefaultValue() == null)
                {
                    property.SetDefaultValue(false);
                }

                break;
            case nameof(IDeletion.DeletionTime):
                if (property.GetValueConverter() == null)
                {
                    SetUnixTimeDefaults(property);
                }

                break;
            case nameof(IDeletion.DeleterId):
                if (property.GetMaxLength() == null)
                {
                    property.SetMaxLength(36);
                }

                break;
            case nameof(IDeletion.DeleterName):
                if (property.GetMaxLength() == null)
                {
                    property.SetMaxLength(256);
                }

                break;
        }
    }

    private static void SetUnixTimeDefaults(IMutableProperty property)
    {
        property.SetValueConverter(new NullableDateTimeOffsetToLongConverter());
        property.SetColumnType("bigint");
        property.IsNullable = true;
    }
}
