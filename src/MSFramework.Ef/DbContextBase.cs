using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Auditing.Configuration;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.Internal;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef;

public abstract class DbContextBase : DbContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly ISession _session;
    protected readonly DbContextConfigurationCollection EntityFrameworkOptions;
    private IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化一个<see cref="DbContextBase"/>类型的新实例
    /// </summary>
    protected DbContextBase(DbContextOptions options,
        IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
        IMediator mediator,
        ISession session, ILoggerFactory loggerFactory)
        : base(options)
    {
        _mediator = mediator;
        EntityFrameworkOptions = entityFrameworkOptions.Value;
        _session = session;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    /// <summary>
    /// 每次新 DbContext 对象都会调用
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var option = EntityFrameworkOptions.Get(GetType());

        Database.AutoTransactionsEnabled = option.AutoTransactionsEnabled;

        if (option.EnableSensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        optionsBuilder.UseLoggerFactory(_loggerFactory);
    }

    /// <summary>
    /// 只会调用一次，创建上下文数据模型时，对各个实体类的数据库映射细节进行配置
    /// </summary>
    /// <param name="modelBuilder">上下文数据模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _entityConfigurationTypeFinder = this.GetService<IEntityConfigurationTypeFinder>();

        //通过实体配置信息将实体注册到当前上下文
        var contextType = GetType();

        var entityTypeConfigurations = _entityConfigurationTypeFinder
            .GetEntityTypeConfigurations(contextType);
        var count = 0;
        var stringBuilder = new StringBuilder();

        foreach (var entityTypeConfiguration in entityTypeConfigurations)
        {
            if (EfRuntimeUtilities.IsDesignTime &&
                entityTypeConfiguration.Value.EntityTypeConfiguration is IExternalMeta
                {
                    IsExternal: true
                })
            {
                continue;
            }

            entityTypeConfiguration.Value.MethodInfo.Invoke(modelBuilder,
                new[] { entityTypeConfiguration.Value.EntityTypeConfiguration });

            if (stringBuilder.Length == 0)
            {
                stringBuilder.Append($"{entityTypeConfiguration.Value.EntityType.FullName}");
            }
            else
            {
                stringBuilder.Append($"、{entityTypeConfiguration.Value.EntityType.FullName}");
            }

            count++;
        }

        if (entityTypeConfigurations.All(x => x.Value.EntityType != typeof(AuditOperation)))
        {
            modelBuilder.ApplyConfiguration(AuditOperationConfiguration.Instance);
        }

        if (entityTypeConfigurations.All(x => x.Value.EntityType != typeof(AuditEntity)))
        {
            modelBuilder.ApplyConfiguration(AuditEntityConfiguration.Instance);
        }

        if (entityTypeConfigurations.All(x => x.Value.EntityType != typeof(AuditProperty)))
        {
            modelBuilder.ApplyConfiguration(AuditPropertyConfiguration.Instance);
        }

        var option = EntityFrameworkOptions.Get(GetType());
        var tablePrefix = option.TablePrefix?.Trim();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // owned 的类型是合并到其主表中，所以没有 table name，schema 的说法
            if (!entityType.IsOwned())
            {
                var defaultTableName = entityType.GetDefaultTableName();
                var tableName = entityType.GetTableName();

                // 只有在用户没有设置自定义表名的情况下，才会自动调整表名
                if (defaultTableName == tableName)
                {
                    if (option.UseUnderScoreCase)
                    {
                        tableName = tableName.ToSnakeCase();
                    }

                    if (!string.IsNullOrWhiteSpace(tablePrefix))
                    {
                        tableName = tablePrefix + tableName;
                    }

                    entityType.SetTableName(tableName);
                }

                // var schema = entityType.GetSchema();
                // schema = string.IsNullOrWhiteSpace(schema) ? option.Schema : schema;
                // entityType.SetSchema(schema);

                // 并不需要，默认规则是 FK_表名_属性名...，因为表名已经有了前缀，所以不会重复
                // if (!string.IsNullOrWhiteSpace(tablePrefix))
                // {
                //     // entityType.GetKeys()
                //     // 在 PG 中，schema 进行了物理管理，不需要考虑 KEY Name 重重的情况
                //     // 在 MySql 中，KEY 的 name 不是全局重复约束
                //
                //     foreach (var key in entityType.GetForeignKeys())
                //     {
                //         // 若是用户自己设置了名称则不自动更新
                //         var customize = key.GetConstraintName();
                //         if (!string.IsNullOrWhiteSpace(customize) && !customize.StartsWith("FK_"))
                //         {
                //             continue;
                //         }
                //
                //         var defaultName = key.GetDefaultName();
                //         key.SetConstraintName($"{tablePrefix}{defaultName}");
                //     }
                //
                //     foreach (var index in entityType.GetIndexes())
                //     {
                //         // 若是用户自己设置了名称则不自动更新
                //         var customize = index.GetDatabaseName();
                //         if (!string.IsNullOrWhiteSpace(customize) && !customize.StartsWith("IX_"))
                //         {
                //             continue;
                //         }
                //
                //         var defaultName = index.GetDefaultDatabaseName();
                //         index.SetDatabaseName($"{tablePrefix}{defaultName}");
                //     }
                // }

                if (typeof(IDeletion).IsAssignableFrom(entityType.ClrType))
                {
                    entityType.AddSoftDeleteQueryFilter();
                }
            }

            var optimisticLockTable = typeof(IOptimisticLock).IsAssignableFrom(entityType.ClrType);
            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                if (optimisticLockTable &&
                    property.Name == "ConcurrencyStamp")
                {
                    property.SetMaxLength(36);
                    property.IsConcurrencyToken = true;
                    property.IsNullable = true;
                }

                if (option.UseUnderScoreCase)
                {
                    var storeObjectIdentifier = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
                    var propertyName = property.GetColumnName(storeObjectIdentifier.GetValueOrDefault());
                    if (!string.IsNullOrEmpty(propertyName) && propertyName.StartsWith("_"))
                    {
                        propertyName = propertyName.Substring(1, propertyName.Length - 1);
                    }

                    property.SetColumnName(propertyName.ToSnakeCase());
                }

                if (property.ClrType == typeof(ObjectId))
                {
                    property.SetValueConverter(new ObjectIdToStringConverter());
                }
            }
        }

        _logger.LogInformation(
            $"将 {count} 个实体 {stringBuilder} 注册到上下文 {contextType} 中");
    }

    public IEnumerable<AuditEntity> GetAuditEntities()
    {
        var entities = new List<AuditEntity>();
        foreach (var entry in ChangeTracker.Entries())
        {
            var auditEntity = entry.State switch
            {
                EntityState.Added => GetAuditEntity(entry, OperationType.Add),
                EntityState.Modified => GetAuditEntity(entry, OperationType.Modify),
                EntityState.Deleted => GetAuditEntity(entry, OperationType.Delete),
                _ => null
            };

            if (auditEntity != null)
            {
                entities.Add(auditEntity);
            }
        }

        return entities;
    }


    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        // 若是有领域事件则分发出去
        // 领域事件可能导致别聚合调用当前 DbContext 并改变状态，或者添加新的事件
        var domainEvents = GetDomainEvents();
        foreach (var @event in domainEvents)
        {
            await _mediator.PublishAsync(@event, cancellationToken);
        }

        var effectedCount = 0;
        var changed = ApplyConcepts();
        if (!changed)
        {
            return effectedCount;
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // 若是有领域事件则分发出去
        // 领域事件可能导致别聚合调用当前 DbContext 并改变状态，或者添加新的事件
        var domainEvents = GetDomainEvents();
        foreach (var @event in domainEvents)
        {
            _mediator.PublishAsync(@event).ConfigureAwait(false);
        }

        var effectedCount = 0;
        var changed = ApplyConcepts();
        return !changed ? effectedCount : base.SaveChanges(acceptAllChangesOnSuccess);
    }

    protected virtual bool ApplyConcepts()
    {
        var userId = _session.UserId;
        var changed = false;

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyConceptsForAddedEntity(entry, userId, _session.UserName);
                    changed = true;
                    break;
                case EntityState.Modified:
                    ApplyConceptsForModifiedEntity(entry, userId, _session.UserName);
                    changed = true;
                    break;
                case EntityState.Deleted:
                    ApplyConceptsForDeletedEntity(entry, userId, _session.UserName);
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
            ? MicroserviceFramework.Defaults.JsonHelper.Serialize(value)
            : value.ToString();
    }

    protected virtual void ApplyConceptsForAddedEntity(EntityEntry entry, string userId, string userName)
    {
        if (entry.Entity is ICreation entity)
        {
            entity.SetCreation(userId);
        }

        if (entry.Entity is IHasCreatorName setter)
        {
            setter.SetProperty(nameof(IHasCreatorName.CreatorName), userName);
        }
    }

    protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry, string userId, string userName)
    {
        if (entry.Entity is IModification entity)
        {
            entity.SetModification(userId);
        }

        if (entry.Entity is IHasLastModifierName setter)
        {
            setter.SetProperty(nameof(IHasLastModifierName.LastModifierName), userName);
        }
    }

    protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry, string userId, string userName)
    {
        if (entry.Entity is IDeletion entity)
        {
            entry.Reload();
            entry.State = EntityState.Modified;

            entity.Delete(userId);
        }

        if (entry.Entity is IHasDeleterName setter)
        {
            setter.SetProperty(nameof(IHasDeleterName.DeleterName), userName);
        }
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
