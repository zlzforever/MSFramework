using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.Internal;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef;

/// <summary>
///
/// </summary>
public abstract class DbContextBase : DbContext
{
    // private readonly ISession _session;
    //
    // // private IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;
    // private readonly IMediator _mediator;

    // private readonly ConcurrentDictionary<Type, List<Type>> _contextInterfaceTypes = new();

    /// <summary>
    /// 初始化一个<see cref="DbContextBase"/>类型的新实例
    /// </summary>
    protected DbContextBase(DbContextOptions options)
        : base(options)
    {
        // _mediator = mediator;
        // _session = session;
    }

    // /// <summary>
    // /// 每次新 DbContext 对象都会调用
    // /// </summary>
    // /// <param name="optionsBuilder"></param>
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     base.OnConfiguring(optionsBuilder);
    //
    //     var option = _entityFrameworkOptions.Get(GetType());
    //     Database.AutoTransactionBehavior = option.AutoTransactionBehavior;
    //     optionsBuilder.UseLoggerFactory(_loggerFactory);
    // }

    /// <summary>
    /// 只会调用一次，创建上下文数据模型时，对各个实体类的数据库映射细节进行配置
    /// </summary>
    /// <param name="modelBuilder">上下文数据模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var settings = this.GetService<DbContextSettings>();
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(DbContextSettings));
        }

        var entityConfigurationTypeFinder = this.GetService<IEntityConfigurationTypeFinder>();

        //通过实体配置信息将实体注册到当前上下文
        var contextType = GetType();

        // var interfaces = _contextInterfaceTypes.GetOrAdd(contextType, type =>
        // {
        //     var baseType = typeof(IDbContext);
        //     var list = type.GetInterfaces()
        //         .Where(x => baseType.IsAssignableFrom(x)).ToList();
        //     list.Add(type);
        //     return list;
        // });

        // foreach (var type in interfaces)
        // {
        //     var entityTypeConfigurations = _entityConfigurationTypeFinder
        //         .GetEntityTypeConfigurations(type);
        //
        //     foreach (var entityTypeConfiguration in entityTypeConfigurations)
        //     {
        //         entityTypeConfiguration.Value.MethodInfo.Invoke(modelBuilder,
        //             [entityTypeConfiguration.Value.EntityTypeConfiguration]);
        //     }
        // }

        modelBuilder.HasAnnotation("DatabaseType", settings.DatabaseType);

        var entityTypeConfigurations = entityConfigurationTypeFinder.GetEntityTypeConfigurations(contextType);
        foreach (var entityTypeConfiguration in entityTypeConfigurations)
        {
            entityTypeConfiguration.EntityTypeConfiguration.Configure(modelBuilder);
        }

        // 需要保证执行顺序，使用虚方法较为合适
        ApplyConfiguration(modelBuilder);

        var tablePrefix = settings.TablePrefix?.Trim();
        foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes())
        {
            // owned 的类型是合并到其主表中， 所以没有 table name，schema 的说法
            if (!mutableEntityType.IsOwned())
            {
                var defaultTableName = mutableEntityType.GetDefaultTableName();
                var tableName = mutableEntityType.GetTableName();

                // 只有在用户没有设置自定义表名的情况下，才会自动调整表名
                if (defaultTableName == tableName)
                {
                    if (settings.UseUnderScoreCase)
                    {
                        tableName = tableName.ToSnakeCase();
                    }

                    if (!string.IsNullOrEmpty(tablePrefix))
                    {
                        tableName = tablePrefix + tableName;
                    }

                    mutableEntityType.SetTableName(tableName);
                }

                // 编译模型不支持使用全局过滤器
                if (!settings.UseCompiledModel &&
                    typeof(IDeletion).IsAssignableFrom(mutableEntityType.ClrType))
                {
                    mutableEntityType.AddSoftDeleteQueryFilter();
                }

                // 继承了实体 & 有 ID 属性 & 未设置主键的实体， 需要自动注入主键
                // 如果有属性 ID， 且已经注入， 且实体已经有主键， 则不需要再管 ID 属性
                var idPropertyInfo = mutableEntityType.ClrType.GetProperties()
                    .FirstOrDefault(x => "id".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                if (Defaults.Types.Entity.IsAssignableFrom(mutableEntityType.ClrType) && idPropertyInfo != null &&
                    mutableEntityType.FindPrimaryKey() == null)
                {
                    var propertyBuilder =
                        mutableEntityType.AddProperty(idPropertyInfo.Name);
                    propertyBuilder.IsPrimaryKey();
                }
            }

            var optimisticLockTable = Defaults.Types.OptimisticLock.IsAssignableFrom(mutableEntityType.ClrType);
            var enumerationToStringConverterType = typeof(EnumerationToStringConverter<>);
            foreach (var property in mutableEntityType.GetProperties())
            {
                if (optimisticLockTable && property.Name == nameof(IOptimisticLock.ConcurrencyStamp))
                {
                    var maxLength = property.GetMaxLength();
                    if (maxLength == null)
                    {
                        property.SetMaxLength(36);
                    }

                    property.IsConcurrencyToken = true;
                    property.IsNullable = true;
                }

                if (settings.UseUnderScoreCase)
                {
                    var storeObjectIdentifier = StoreObjectIdentifier.Table(mutableEntityType.GetTableName(),
                        mutableEntityType.GetSchema());
                    var propertyName = property.GetColumnName(storeObjectIdentifier);
                    if (!string.IsNullOrEmpty(propertyName) && propertyName.StartsWith('_'))
                    {
                        propertyName = propertyName.Substring(1, propertyName.Length - 1);
                    }

                    property.SetColumnName(propertyName.ToSnakeCase());
                }

                if (property.ClrType == Defaults.Types.ObjectId)
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
                else if (property.ClrType == Defaults.Types.Guid)
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
                else if (property.ClrType.IsAssignableTo(typeof(Enumeration)))
                {
                    var columnType = property.GetColumnType();
                    if (string.IsNullOrEmpty(columnType))
                    {
                        property.SetColumnType("varchar");
                    }

                    var maxLength = property.GetMaxLength();
                    if (maxLength == null)
                    {
                        property.SetMaxLength(256);
                    }

                    var converterType = enumerationToStringConverterType.MakeGenericType(property.ClrType);
                    var converter = (ValueConverter)Activator.CreateInstance(converterType);
                    property.SetValueConverter(converter);
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected abstract void ApplyConfiguration(ModelBuilder modelBuilder);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AuditEntity> GetAuditEntities()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            // 审计对象不被审计
            if (entry.Entity is IAuditObject)
            {
                continue;
            }

            var auditEntity = entry.State switch
            {
                EntityState.Added => GetAuditEntity(entry, OperationType.Add),
                EntityState.Modified => GetAuditEntity(entry, OperationType.Modify),
                EntityState.Deleted => GetAuditEntity(entry, OperationType.Delete),
                _ => null
            };

            if (auditEntity != null)
            {
                yield return auditEntity;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        var scopeServiceProvider = this.GetService<IScopeServiceProvider>();
        var mediator = scopeServiceProvider.GetService<IMediator>();
        if (mediator != null)
        {
            // 若是有领域事件则分发出去
            // 领域事件可能导致别聚合调用当前 DbContext 并改变状态，或者添加新的事件
            var domainEvents = GetDomainEvents();
            foreach (var @event in domainEvents)
            {
                await mediator.PublishAsync(@event, cancellationToken);
            }
        }

        var effectedCount = 0;
        var changed = ApplyConcepts();
        if (!changed)
        {
            return effectedCount;
        }

        var r = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        return r;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <returns></returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var scopeServiceProvider = this.GetService<IScopeServiceProvider>();
        var mediator = scopeServiceProvider.GetService<IMediator>();
        if (mediator != null)
        {
            // 若是有领域事件则分发出去
            // 领域事件可能导致别聚合调用当前 DbContext 并改变状态，或者添加新的事件
            var domainEvents = GetDomainEvents();
            foreach (var @event in domainEvents)
            {
                mediator.PublishAsync(@event).ConfigureAwait(false);
            }
        }

        var effectedCount = 0;
        var changed = ApplyConcepts();
        return !changed ? effectedCount : base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected virtual bool ApplyConcepts()
    {
        var scopeServiceProvider = this.GetService<IScopeServiceProvider>();
        var session = scopeServiceProvider.GetService<ISession>();
        var userId = session?.UserId;
        var name = session?.UserDisplayName;
        var changed = false;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IExternalEntity)
            {
                entry.State = EntityState.Unchanged;
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyConceptsForAddedEntity(entry, userId, name);
                    changed = true;
                    break;
                case EntityState.Modified:
                    ApplyConceptsForModifiedEntity(entry, userId, name);
                    changed = true;
                    break;
                case EntityState.Deleted:
                    ApplyConceptsForDeletedEntity(entry, userId, name);
                    changed = true;
                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return changed;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="operationType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected virtual AuditEntity GetAuditEntity(EntityEntry entry, OperationType operationType)
    {
        var type = entry.Entity.GetType();
        var typeName = type.FullName;

        string entityId = null;
        var properties = new List<AuditProperty>();
        foreach (var property in entry.CurrentValues.Properties)
        {
            if (property.IsConcurrencyToken)
            {
                continue;
            }

            var propertyName = property.Name;
            var propertyEntry = entry.Property(property.Name);
            if (property.IsPrimaryKey())
            {
                entityId = entry.State == EntityState.Deleted
                    ? propertyEntry.OriginalValue?.ToString()
                    : propertyEntry.CurrentValue?.ToString();
            }

            var propertyType = property.ClrType.ToString();
            string originalValue = null;
            string newValue = null;

            var columnType = propertyEntry.Metadata.GetColumnType();
            switch (entry.State)
            {
                case EntityState.Added:
                    newValue = GetValue(columnType, propertyEntry.CurrentValue);
                    break;
                case EntityState.Deleted:
                    originalValue = GetValue(columnType, propertyEntry.OriginalValue);
                    break;
                case EntityState.Modified:
                {
                    var currentValue = GetValue(columnType, propertyEntry.CurrentValue);
                    originalValue = GetValue(columnType, propertyEntry.OriginalValue);
                    if (currentValue == originalValue)
                    {
                        continue;
                    }

                    newValue = currentValue;
                    break;
                }
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (string.IsNullOrWhiteSpace(originalValue))
            {
                // 原值为空，新值不为空则记录
                if (!string.IsNullOrWhiteSpace(newValue))
                {
                    properties.Add(new AuditProperty(propertyName, propertyType, originalValue, newValue));
                }
            }
            else
            {
                if (!originalValue.Equals(newValue))
                {
                    properties.Add(new AuditProperty(propertyName, propertyType, originalValue, newValue));
                }
            }
        }

        var auditedEntity = new AuditEntity(typeName, entityId, operationType);
        auditedEntity.AddProperties(properties);
        return auditedEntity;
    }

    private string GetValue(string columnType, object value)
    {
        if (value == null)
        {
            return null;
        }

        return Regex.IsMatch(columnType, "JSON", RegexOptions.IgnoreCase)
            ? Defaults.JsonSerializer.Serialize(value)
            : value.ToString();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    protected virtual void ApplyConceptsForAddedEntity(EntityEntry entry, string userId, string userName)
    {
        if (entry.Entity is ICreation entity)
        {
            entity.SetCreation(userId, userName);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry, string userId, string userName)
    {
        if (entry.Entity is IModification entity)
        {
            entity.SetModification(userId, userName);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry, string userId, string userName)
    {
        if (entry.Entity is not IDeletion entity)
        {
            return;
        }

        entry.Reload();
        entry.State = EntityState.Modified;
        entity.SetDeletion(userId, userName);
    }

    private List<DomainEvent> GetDomainEvents()
    {
        // Dispatch Domain Events collection.
        // Choices:
        // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including
        // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
        // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions.
        // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers.

        // 此处不能改为迭代器， 事件在迭代过程中会触发 ChangeTracker.Entries 的变化
        var domainEvents = new List<DomainEvent>();

        foreach (var aggregateRoot in ChangeTracker
                     .Entries<EntityBase>())
        {
            var events = aggregateRoot.Entity.GetDomainEvents();
            if (events != null && events.Any())
            {
                domainEvents.AddRange(events);
                aggregateRoot.Entity.ClearDomainEvents();
            }
        }

        return domainEvents;
    }
}
