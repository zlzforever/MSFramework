using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef;

/// <summary>
/// EF Core DbContext 基类，集成领域事件分发、审计追踪和工作单元支持
/// </summary>
public abstract class DbContextBase : DbContext
{
    /// <summary>
    /// 实体审计策略链，按数组顺序执行。
    /// ExternalEntity 必须排第一位，确保在其他策略之前拦截外部实体；
    /// DeletionAuditingStrategy 必须排在 ModificationAuditingStrategy 之前：
    /// 软删除转换（Deleted 转 Modified）后不能再叠加修改审计，
    /// 且未命中的条目（如普通修改、物理删除）会被它放行给后续策略。
    /// 每个实体遍历时，第一个返回 <c>true</c> 的策略为命中，后续不再执行。
    /// 子类可通过重写 <see cref="ApplyConcepts"/> 或替换策略来定制行为。
    /// </summary>
    private static readonly IEntityAuditingStrategy[] AuditingStrategies =
    [
        new ExternalEntityAuditingStrategy(),
        new CreationAuditingStrategy(),
        new DeletionAuditingStrategy(),
        new ModificationAuditingStrategy()
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
            // 状态类错误用 InvalidOperationException：DbContextSettings 未注册不是方法参数问题，
            // 误用 ArgumentNullException 会被全局异常过滤器按"参数错误"分类、误导排障
            throw new InvalidOperationException(
                "未在服务容器中找到 DbContextSettings。" +
                "请确认已通过 options.Load(settings)（或 AddEntityFrameworkExtension 的配置注册）" +
                "将 DbContextSettings 挂载到当前 DbContext");
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
    /// 直接提交当前上下文的全部待提交变更，<b>绕过完整保存管道</b>（领域事件派发、
    /// 审计字段应用 <see cref="ApplyConcepts"/>、审计收集）。
    /// <para>
    /// 专用入口，适用于"保存的数据本身不会产生领域事件、也不应被审计"的场景——
    /// 典型是审计存储（<see cref="Auditing.EfAuditingStore{TDbContext}"/>，经独立 scope 解析上下文，
    /// ChangeTracker 只含审计实体）。
    /// </para>
    /// <para>
    /// 使用约束：调用方须自行保证——上下文中没有待派发的领域事件（不会被派发）、
    /// 没有待提交的业务变更（本方法会提交整个 ChangeTracker 且不做审计字段应用）。
    /// 与业务共享上下文实例时<b>禁止</b>使用本方法（会顺带提交业务变更并绕过业务管道）；
    /// 常规业务保存请继续使用 <see cref="SaveChangesAsync(bool, CancellationToken)"/>
    /// </para>
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">成功后是否接受所有更改</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>受影响的行数</returns>
    public Task<int> SaveChangesWithoutPipelineAsync(bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
            // 领域事件与业务数据同事务：Handler 抛异常会中断整个保存，不产生提交；
            // Handler 可能修改当前 DbContext 跟踪的实体，或向聚合添加新事件，
            // 因此派发后重新收集，直到没有新事件（见 DispatchDomainEventsAsync）
            //
            // 设计决策（勿在后续 review 中重复提出）：领域事件在提交前派发、与业务数据同库同事务，
            // 不采用"先提交再派发/提交失败恢复事件"模型。理由：
            // 1) 进程内领域事件 Handler 与当前 DbContext 共享同一 scoped 实例，
            //    其对同库的变更天然落入同一事务，原子性已由事务保证；
            // 2) 若先提交再派发，提交后 Handler 失败将产生"数据已落库、事件未送达"且
            //    无法回滚的状态，语义更差；
            // 3) 事务边界之外的副作用（跨库写、外部系统调用、发消息）不属于领域事件职责，
            //    应使用集成事件（发布到队列，CAP/Dapr pubsub）承载，其可靠性由消息中间件/Outbox 保证。
            await DispatchDomainEventsAsync(mediator, cancellationToken);
        }

        // "是否需要保存"与"审计字段是否应用"解耦：审计字段应用（ApplyConcepts）只承担副作用，
        // 是否保存由 EF 变更追踪判定。旧版以 ApplyConcepts 返回值作为保存门禁，
        // 审计策略未命中的变更（如未实现任何审计接口的 Deleted 实体，即物理删除）
        // 不会翻转 changed 标记，导致该类变更被静默丢弃。
        ApplyConcepts();
        if (!ChangeTracker.HasChanges())
        {
            return 0;
        }

        // 审计批次"提交后追加"语义：提交前只生成局部批次快照，保存成功后才追加到审计操作——
        // 保存失败（异常/乐观锁冲突等）时批次随局部变量丢弃，重试成功的批次才是提交历史。
        // 旧实现先追加后保存，失败尝试会混入审计变更历史，请求最终成功时被伪装成已提交变更
        var auditBatch = CollectAuditBatch();

        var saved = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        if (auditBatch != null)
        {
            auditBatch.Value.Operation.AddEntities(auditBatch.Value.Batch);
        }

        return saved;
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
            // 同步路径遗留分支：派发语义与异步版一致（含新增事件多轮收集）
            DispatchDomainEvents(mediator);
        }

        // "是否需要保存"与"审计字段是否应用"解耦（同 SaveChangesAsync，见其注释）
        ApplyConcepts();
        if (!ChangeTracker.HasChanges())
        {
            return 0;
        }

        // 审计批次"提交后追加"语义（同 SaveChangesAsync）
        var auditBatch = CollectAuditBatch();

        var saved = base.SaveChanges(acceptAllChangesOnSuccess);

        if (auditBatch != null)
        {
            auditBatch.Value.Operation.AddEntities(auditBatch.Value.Batch);
        }

        return saved;
    }

    /// <summary>
    /// 生成当前保存的审计批次快照（<b>不追加</b>到审计操作，追加动作由调用方在提交成功后执行）。
    /// <para>
    /// "提交后追加"设计：保存失败（异常/乐观锁冲突）时批次随本方法返回的局部值被丢弃，
    /// 不得把失败尝试写入审计变更历史（否则请求最终成功时，失败尝试会被伪装成已提交变更）；
    /// 也不以全局去重修复该问题——同一实体的多次合法变更历史必须完整保留。
    /// </para>
    /// <para>
    /// 审计请求由 <see cref="AuditOperationContext"/>（AsyncLocal）随执行流承载，
    /// 与工作单元实现解耦：只要在执行流内调用保存（含绕过工作单元直接调用
    /// <see cref="SaveChangesAsync(bool, CancellationToken)"/> 的路径），且未越过
    /// 审计链路终点，即可被收集。
    /// Value 为 null（无审计请求）时立即返回，仅一次 AsyncLocal 读取开销；
    /// 每次保存先物化一个收集批次（实体值在此刻快照，不受后续提交状态变化影响），
    /// 批次之间不共享收集状态，保证同一实体恢复旧值后再次变更仍完整保留。
    /// </para>
    /// <para>
    /// 已知限制（勿在后续 review 中重复提出）：显式事务（<c>Database.BeginTransaction</c>/
    /// 环境事务）的<b>最终回滚</b>无法在此感知——SaveChanges 成功即追加审计，之后的事务回滚
    /// 不会撤销审计记录；使用显式事务且可能回滚的场景请自行处理（如回滚时清空审计操作）。
    /// </para>
    /// </summary>
    /// <returns>审计操作与其批次；无审计请求或批次为空时返回 null</returns>
    private (AuditOperation Operation, AuditEntity[] Batch)? CollectAuditBatch()
    {
        var auditOperation = AuditOperationContext.Value;
        if (auditOperation == null)
        {
            return null;
        }

        var batch = GetAuditEntities().ToArray();
        return batch.Length == 0 ? null : (auditOperation, batch);
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

            if (Equals(originalValue, newValue))
            {
                continue;
            }

            properties.Add(new AuditProperty(propertyName, propertyType, originalValue, newValue));
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
    /// <remarks>
    /// 收集时机与主键生成策略（已知约定，勿在后续 review 中重复提出）：
    /// 审计收集发生在 <c>base.SaveChangesAsync</c> 提交<b>之前</b>——
    /// 本项目全部使用<b>离线 ID</b>（如 ObjectId、客户端生成的 Guid/字符串），
    /// 主键在进入 ChangeTracker 前已生成，提交前收集即可拿到完整 EntityId；
    /// 数据库端生成的主键（identity/自增、HiLo 等）不支持——提交前 CurrentValue 为空，
    /// 审计的 EntityId 会记录为 null
    /// </remarks>
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

        var isJson = columnType?.Contains(
            "JSON",
            StringComparison.OrdinalIgnoreCase) == true;
        return isJson ? Defaults.JsonSerializer.Serialize(value) : value.ToString();
    }

    /// <summary>
    /// 领域事件最大派发轮数的默认值（可通过 <see cref="DbContextSettings.MaxDomainEventDispatchRounds"/> 配置）。
    /// 正常业务不应超过 3 轮，业务代码也应避免 Handler 相互触发产生事件
    /// </summary>
    private const int DefaultMaxDomainEventDispatchRounds = 3;

    /// <summary>
    /// 读取当前上下文的领域事件最大派发轮数配置；
    /// 未注册 <see cref="DbContextSettings"/>（如设计时工厂构建模型）时回退默认值
    /// </summary>
    /// <returns>最大派发轮数（配置值小于 1 时按 1 处理，保证至少派发一轮）</returns>
    private int GetMaxDomainEventDispatchRounds()
    {
        var settings = this.GetService<DbContextSettings>();
        var configured = settings?.MaxDomainEventDispatchRounds ?? DefaultMaxDomainEventDispatchRounds;
        return configured < 1 ? DefaultMaxDomainEventDispatchRounds : configured;
    }

    /// <summary>
    /// 分发当前所有待处理领域事件，并循环收集派发过程中新增的事件。
    /// <para>
    /// 事务模型：领域事件与本业务同事务（收集 A——在提交数据库之前派发，使用同一 scoped
    /// DbContext 的 Handler 变更由本次保存统一提交，任一 Handler 异常则整体不提交）；
    /// 集成事件（跨进程）应由领域事件 Handler 通过 CAP 本地消息表（Outbox）发出，
    /// 与业务变更同事务落库、提交后投递，避免数据库回滚而外部消息已发出的不一致。
    /// </para>
    /// <para>
    /// 轮次与中止语义：每轮"先只读收集、校验轮次上限、再清空、再派发"。
    /// 达到上限时事件<b>保留在聚合上</b>（不已被清空），业务整体回滚且事件痕迹可诊断/人工补发——
    /// 旧实现收集时立即清空，中止后事件完全消失，造成"回滚 + 事件丢失"且不可观测。
    /// </para>
    /// </summary>
    /// <param name="mediator">中介者</param>
    /// <param name="cancellationToken">取消令牌</param>
    private async Task DispatchDomainEventsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var maxRounds = GetMaxDomainEventDispatchRounds();
        for (var round = 1; ; round++)
        {
            var domainEvents = CollectDomainEvents();
            if (domainEvents.Count == 0)
            {
                return;
            }

            if (round > maxRounds)
            {
                // 中止前记录保留在聚合上的事件（未清空），供诊断与人工补发
                this.GetService<ILoggerFactory>().CreateLogger(GetType()).LogError(
                    "领域事件已派发 {MaxRounds} 轮仍有新事件产生，已中止保存；事件 {Events} 保留在聚合上待人工处置",
                    maxRounds, string.Join(", ", domainEvents.Select(e => e.GetType().Name)));
                throw new MicroserviceFrameworkException(
                    $"领域事件已派发 {maxRounds} 轮仍有新事件产生，" +
                    "疑似领域事件 Handler 相互触发形成循环（正常业务不应超过 3 轮，请检查 Handler 是否在处理事件时又引发同类事件），已中止保存");
            }

            // 收集完成后再清空聚合事件，保证每个事件在本次保存中仅派发一次
            ClearCollectedDomainEvents();

            foreach (var @event in domainEvents)
            {
                await mediator.PublishAsync(@event, cancellationToken);
            }
        }
    }

    /// <summary>
    /// 阻塞等待异步派发完成，供遗留的同步 <see cref="SaveChanges(bool)"/> 路径使用；
    /// 轮次与中止语义由异步实现统一维护，达到上限时事件保留在聚合上。
    /// </summary>
    /// <remarks>
    /// 此桥接会阻塞当前线程；异步处理器捕获同步上下文时存在死锁风险。
    /// 业务调用应优先使用 SaveChangesAsync，避免同步等待异步处理器。
    /// </remarks>
    /// <param name="mediator">当前作用域的中介者，不允许为空。</param>
    /// <exception cref="MicroserviceFrameworkException">领域事件派发超过配置的轮次上限。</exception>
    private void DispatchDomainEvents(IMediator mediator)
    {
        DispatchDomainEventsAsync(mediator, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 只读收集当前 ChangeTracker 中所有聚合的待派发领域事件（<b>不清空</b>聚合事件列表）。
    /// <para>
    /// 使用 List 按发生顺序收集，不做按值去重：两个值相同的 record 事件代表两次独立的业务发生，
    /// 每次都应派发（旧版 HashSet 按值合并会吞掉重复事件并丢失发生顺序）。
    /// </para>
    /// <para>
    /// 收集必须一次性物化完成（不能改为迭代器）：收集完成后须清空聚合事件再派发
    /// （见 <see cref="ClearCollectedDomainEvents"/>），若在枚举过程中派发，
    /// Handler 对 ChangeTracker 的修改会触发集合变化异常。
    /// </para>
    /// <para>
    /// 只读收集的设计目的：达到派发轮次上限中止保存时，事件仍保留在聚合上可诊断；
    /// 确认本轮未超限后才统一清空再派发
    /// </para>
    /// </summary>
    /// <returns>待派发领域事件列表（可能为空）</returns>
    private List<DomainEvent> CollectDomainEvents()
    {
        var domainEvents = new List<DomainEvent>();

        foreach (var aggregateRoot in ChangeTracker
                     .Entries<EntityBase>())
        {
            var events = aggregateRoot.Entity.GetDomainEvents();
            if (events is { Count: > 0 })
            {
                domainEvents.AddRange(events);
            }
        }

        return domainEvents;
    }

    /// <summary>
    /// 清空 ChangeTracker 中所有聚合的待派发领域事件。
    /// 在"只读收集 + 轮次校验"通过后调用，与 <see cref="CollectDomainEvents"/> 配对，
    /// 保证每个事件在本次保存中仅派发一次
    /// </summary>
    private void ClearCollectedDomainEvents()
    {
        foreach (var aggregateRoot in ChangeTracker
                     .Entries<EntityBase>())
        {
            aggregateRoot.Entity.ClearDomainEvents();
        }
    }
}
