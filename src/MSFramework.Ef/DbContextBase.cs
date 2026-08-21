using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Auditing;
using MicroserviceFramework.Ef.Internal;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MicroserviceFramework.Ef;

/// <summary>
/// EF Core DbContext 基类，集成领域事件分发、审计追踪和工作单元支持
/// </summary>
public abstract class DbContextBase : DbContext
{
    /// <summary>
    /// 实体审计策略链，按数组顺序执行。
    /// ExternalEntity 必须排第一位，确保在其他策略之前拦截外部实体。
    /// 每个实体遍历时，第一个返回 <c>true</c> 的策略为命中，后续不再执行。
    /// 子类可通过重写 <see cref="ApplyConcepts"/> 或替换策略来定制行为。
    /// </summary>
    private static readonly IEntityAuditingStrategy[] AuditingStrategies =
    [
        new ExternalEntityAuditingStrategy(),
        new CreationAuditingStrategy(),
        new ModificationAuditingStrategy(),
        new DeletionAuditingStrategy()
    ];

    /// <summary>
    /// 模型构建策略管道，按数组顺序执行。
    /// 每个策略封装 OnModelCreating 中的一个独立配置关注点。
    /// </summary>
    private static readonly IModelBuildingStrategy[] ModelBuildingStrategies =
    [
        new RegisterEntityConfigurationsStrategy(),
        new EntityTableConventionStrategy(),
        new EntityPropertyConventionStrategy()
    ];

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
    /// 只会调用一次，创建上下文数据模型时，对各个实体类的数据库映射细节进行配置。
    /// 通过 <see cref="ModelBuildingStrategies"/> 管道依次执行：
    ///   1. 注册实体类型配置（IEntityTypeConfiguration）
    /// 策略 2: 实体表级约定补全（表名 + 软删除过滤器）
    ///   3. 实体属性级约定补全（列名 + 列类型 + 乐观锁 + 审计字段）
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var settings = this.GetService<DbContextSettings>();
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(DbContextSettings));
        }

        var entityConfigurationTypeFinder = this.GetService<IEntityConfigurationTypeFinder>();
        modelBuilder.HasAnnotation("DatabaseType", settings.DatabaseType);

        // 子类扩展点：在所有内置策略执行完毕后，应用自定义配置
        ApplyConfiguration(modelBuilder);

        // 默认策略应该在最后，用于保证如表、列命名的统一性。
        var ctx = new ModelBuildingContext(modelBuilder, settings, entityConfigurationTypeFinder, GetType());
        foreach (var modelBuildingStrategy in ModelBuildingStrategies)
        {
            modelBuildingStrategy.Apply(ctx);
        }
    }

    /// <summary>
    /// [向后兼容] 表名重命名逻辑已迁移至 <see cref="EntityTableConventionStrategy"/>。
    /// 默认的 <see cref="OnModelCreating"/> 不再调用此方法。
    /// 子类若重写了 <see cref="OnModelCreating"/> 仍可手动调用。
    /// </summary>
    protected virtual void RenameTableName(IMutableEntityType mutableEntityType, DbContextSettings settings)
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

            if (!string.IsNullOrEmpty(settings.TablePrefix))
            {
                tableName = settings.TablePrefix + tableName;
            }

            mutableEntityType.SetTableName(tableName);
        }
    }

    /// <summary>
    /// 子类实现以配置实体映射
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected abstract void ApplyConfiguration(ModelBuilder modelBuilder);

    /// <summary>
    /// 获取变更追踪中的审计实体集合
    /// </summary>
    /// <returns>审计实体枚举</returns>
    public virtual IEnumerable<AuditEntity> GetAuditEntities()
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
    /// 保存所有更改，并在提交前分发领域事件
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">成功后是否接受所有更改</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>受影响的行数</returns>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        var scopeServiceProvider = GetScopeServiceProvider();
        var mediator = scopeServiceProvider?.GetService<IMediator>();
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

        CollectAuditEntities();

        var r = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        return r;
    }

    /// <summary>
    /// EF Core 官方在 .NET 6+ 已经明确表态：SaveChanges 同步方法是遗留产物，推荐全部使用 Async
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <returns></returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var scopeServiceProvider = GetScopeServiceProvider();
        var mediator = scopeServiceProvider?.GetService<IMediator>();
        if (mediator != null)
        {
            // 若是有领域事件则分发出去
            // 领域事件可能导致别聚合调用当前 DbContext 并改变状态，或者添加新的事件
            var domainEvents = GetDomainEvents();
            foreach (var @event in domainEvents)
            {
                // 不会死锁：它等同于 await 的底层机制，不会捕获 SynchronizationContext。ASP.NET Core 内部的 HostingApplication 在处理非 async 中间件时正是这么做的。
                mediator.PublishAsync(@event).GetAwaiter().GetResult();
            }
        }

        var effectedCount = 0;
        var changed = ApplyConcepts();
        if (!changed)
        {
            return effectedCount;
        }

        CollectAuditEntities();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 审计收集挂钩：在实体状态定型（<see cref="ApplyConcepts"/>）之后、提交数据库之前，
    /// 若当前执行流承载审计操作（<see cref="AuditOperationContext.Value"/> 非空），
    /// 则收集变更实体到该操作中。
    /// 审计请求由 <see cref="AuditOperationContext"/>（AsyncLocal）随执行流承载，
    /// 与工作单元实现解耦：只要在执行流内调用保存（含绕过工作单元直接调用
    /// <see cref="SaveChangesAsync(bool, CancellationToken)"/> 的路径），且未越过
    /// 审计链路终点，即可被收集。
    /// Value 为 null（无审计请求）时本方法立即返回，仅一次 AsyncLocal 读取开销；
    /// 实体收集结果经 <see cref="AuditOperation.AddEntities"/> 按值身份去重，
    /// 多次保存/多上下文场景不会重复收集同一实体的同一变更状态。
    /// </summary>
    private void CollectAuditEntities()
    {
        var auditOperation = AuditOperationContext.Value;
        if (auditOperation == null)
        {
            return;
        }

        auditOperation.AddEntities(GetAuditEntities());
    }

    /// <summary>
    /// 将 ChangeTracker 中的实体变化应用到审计字段（创建/修改/删除人信息）。
    /// 策略模式：遍历 <see cref="AuditingStrategies"/> 数组，
    /// 每个实体匹配到第一个命中策略后跳出内层循环。
    /// </summary>
    /// <returns>是否有实体发生了状态变更</returns>
    protected virtual bool ApplyConcepts()
    {
        var scopeServiceProvider = GetScopeServiceProvider();
        var session = scopeServiceProvider?.GetService<ISession>();
        var userId = session?.UserId;
        var name = session?.UserDisplayName;
        var changed = false;

        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var auditingStrategy in AuditingStrategies)
            {
                if (auditingStrategy.Apply(entry, userId, name))
                {
                    changed = true;
                    break;
                }
            }
        }

        return changed;
    }

    /// <summary>
    /// 优先读取后台事件建立的当前异步作用域，HTTP 请求则回退到 EF 注册的作用域服务提供器。
    /// </summary>
    private IScopeServiceProvider GetScopeServiceProvider()
    {
        return ScopeServiceProviderContext.Current ?? this.GetService<IScopeServiceProvider>();
    }

    /// <summary>
    /// 获取实体变更的审计信息。
    /// 实体标识（<see cref="AuditEntity.EntityId"/>）取实体全部主键属性值，
    /// 多主键（复合主键）场景按主键属性顺序以 <c>|</c> 分隔拼接，单主键场景行为与历史版本一致。
    /// </summary>
    /// <param name="entry">实体跟踪条目</param>
    /// <param name="operationType">操作类型</param>
    /// <returns>审计实体信息</returns>
    /// <exception cref="ArgumentOutOfRangeException">操作类型超出预期范围</exception>
    protected virtual AuditEntity GetAuditEntity(EntityEntry entry, OperationType operationType)
    {
        var type = entry.Entity.GetType();
        var typeName = type.FullName;

        var entityId = BuildEntityId(entry);
        var properties = new List<AuditProperty>();
        foreach (var property in entry.CurrentValues.Properties)
        {
            if (property.IsConcurrencyToken)
            {
                continue;
            }

            var propertyName = property.Name;
            var propertyEntry = entry.Property(property.Name);

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

    /// <summary>
    /// 按主键元数据顺序收集实体全部主键属性值，拼接为审计实体标识。
    /// 单主键返回单个值；复合主键以 <c>|</c> 分隔拼接。
    /// 删除（<see cref="EntityState.Deleted"/>）状态下取原始值，其余状态取当前值。
    /// </summary>
    /// <param name="entry">实体跟踪条目</param>
    /// <returns>拼接后的实体标识；实体无主键时返回 null</returns>
    private static string BuildEntityId(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();
        if (primaryKey == null)
        {
            return null;
        }

        var values = primaryKey.Properties.Select(p =>
        {
            var propertyEntry = entry.Property(p.Name);
            return entry.State == EntityState.Deleted
                ? propertyEntry.OriginalValue?.ToString()
                : propertyEntry.CurrentValue?.ToString();
        });
        return string.Join("|", values);
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
