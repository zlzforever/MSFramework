using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MicroserviceFramework.Ef.Auditing;

/// <summary>
/// 乐观锁并发令牌轮换助手：在实体被修改落库前，为实现了
/// <see cref="IOptimisticLock"/> 的实体重新生成 <see cref="IOptimisticLock.ConcurrencyStamp"/>。
/// </summary>
/// <remarks>
/// 令牌被 <c>ModelBuildingStrategies</c> 配置为并发令牌（IsConcurrencyToken）后，
/// EF Core 生成的 UPDATE 形如 <c>SET ConcurrencyStamp = @新值 WHERE ConcurrencyStamp = @原值</c>。
/// 若从不轮换令牌，SET 与 WHERE 恒等，两次并发修改会先后成功（lost update），
/// <see cref="DbUpdateConcurrencyException"/> 永远不会因令牌抛出——因此每次修改命中时
/// 轮换令牌是乐观锁生效的前提，各审计策略必须在写入审计字段后调用本方法。
/// </remarks>
internal static class OptimisticLockHelper
{
    /// <summary>
    /// 轮换乐观锁并发令牌：为实体生成新的 <see cref="IOptimisticLock.ConcurrencyStamp"/> 值。
    /// 未实现 <see cref="IOptimisticLock"/> 的实体不做任何处理。
    /// </summary>
    /// <param name="entity">正被写入数据库的实体实例，允许为 null（直接忽略）</param>
    public static void Rotate(object entity)
    {
        if (entity is IOptimisticLock lockEntity)
        {
            // UUID v7 按时间大致有序（同一毫秒内不保证顺序）；N 格式保持 32 位无连字符。
            lockEntity.ConcurrencyStamp = Guid.CreateVersion7().ToString("N");
        }
    }
}

/// <summary>
/// 外部实体策略：标记 <see cref="IExternalEntity"/> 为 <see cref="EntityState.Unchanged"/>，
/// 阻止 EF Core 对其追踪和生成变更。
/// 必须排在策略数组的第一位，确保在其他策略之前拦截。
/// </summary>
internal sealed class ExternalEntityAuditingStrategy : IEntityAuditingStrategy
{
    public bool Apply(EntityEntry entry, string userId, string userName)
    {
        if (entry.Entity is not IExternalEntity)
        {
            return false;
        }

        entry.State = EntityState.Unchanged;
        return true; // 已处理，跳过后续策略
    }
}

/// <summary>
/// 创建审计策略：处理 <see cref="EntityState.Added"/> 状态的实体，
/// 对实现了 <see cref="ICreation"/> 的实体设置创建人、创建时间。
/// </summary>
internal sealed class CreationAuditingStrategy : IEntityAuditingStrategy
{
    public bool Apply(EntityEntry entry, string userId, string userName)
    {
        if (entry.State != EntityState.Added)
        {
            return false;
        }

        var now = DateTimeOffset.Now;
        if (entry.Entity is ICreation entity)
        {
            entity.SetCreation(userId, userName, now);
        }

        if (entry.Entity is IModification modification)
        {
            modification.SetModification(userId, userName, now);
        }

        return true;
    }
}

/// <summary>
/// 修改审计策略：处理 <see cref="EntityState.Modified"/> 状态的实体，
/// 对实现了 <see cref="IModification"/> 的实体设置最后修改人、最后修改时间，
/// 并轮换乐观锁并发令牌（见 <see cref="OptimisticLockHelper"/>）。
/// 软删除实体（<see cref="IDeletion"/>）由排在前面的 <see cref="DeletionAuditingStrategy"/> 优先处理，
/// 本策略只负责未被其命中的普通修改。
/// </summary>
internal sealed class ModificationAuditingStrategy : IEntityAuditingStrategy
{
    public bool Apply(EntityEntry entry, string userId, string userName)
    {
        if (entry.State != EntityState.Modified)
        {
            return false;
        }

        if (entry.Entity is IModification entity)
        {
            entity.SetModification(userId, userName);
        }

        // 轮换并发令牌：使 UPDATE 同时携带 SET 新值与 WHERE 原值，
        // 并发冲突时第二个提交者 WHERE 不命中而抛 DbUpdateConcurrencyException
        OptimisticLockHelper.Rotate(entry.Entity);

        return true;
    }
}

/// <summary>
/// 删除审计策略（软删除）：处理 <see cref="EntityState.Deleted"/> 状态的实体，
/// 对实现了 <see cref="IDeletion"/> 的实体转 <see cref="EntityState.Modified"/> 并调用
/// <see cref="IDeletion.SetDeletion"/> 写入删除审计字段，实现软删除而非物理删除；
/// 同时处理 Modified 状态下 IsDeleted 由 false 变为 true 的领域显式标记删除场景。
/// 命中处理后返回 true 阻止后续策略；未命中一律返回 false，交由后续策略（如修改审计）处理。
/// </summary>
/// <remarks>
/// 聚合根集合移除子项的场景要求关系为必需且 DeleteBehavior 为 ClientCascade
/// （见 <c>ReferenceCollectionBuilderExtensions.OnSoftDelete</c>）：
/// EF Core 会把被移除的子项标记为 <see cref="EntityState.Deleted"/> 且外键值保留，
/// 本策略在其落库前转回 Modified 实现软删除，外键列不会被置空；
/// 数据库外键禁止配置级联删除（<see cref="DeleteBehavior.Cascade"/>），防止真实 DELETE 扩散。
/// </remarks>
internal sealed class DeletionAuditingStrategy : IEntityAuditingStrategy
{
    /// <summary>
    /// 删除审计属性名集合：由 <see cref="IDeletion"/> 契约强制定义，
    /// 软删除 UPDATE 必须包含这些列（含 <see cref="IDeletion.IsDeleted"/>）。
    /// </summary>
    private static readonly HashSet<string> DeletionAuditPropertyNames =
    [
        nameof(IDeletion.IsDeleted),
        nameof(IDeletion.DeleterId),
        nameof(IDeletion.DeleterName),
        nameof(IDeletion.DeletionTime)
    ];

    public bool Apply(EntityEntry entry, string userId, string userName)
    {
        // 聚合根集合移除子项：必需关系 + ClientCascade 时，EF 把被移除的子项标记为 Deleted（外键值保留）。
        // 转 Modified 实现软删除：实体被标记 Deleted 后属性值仍保留在内存中，
        // 直接转 Modified 并设置删除审计字段即可，省去 Reload 带来的逐行 SELECT。
        if (entry.State == EntityState.Deleted)
        {
            if (entry.Entity is not IDeletion entity)
            {
                return false; // 物理删除实体，交由后续策略处理
            }

            entry.State = EntityState.Modified;
            entity.SetDeletion(userId, userName);

            // 轮换并发令牌：软删除的 UPDATE 同样应受乐观锁保护（见 OptimisticLockHelper）；
            // 必须在下方 IsModified 收敛循环之前轮换——令牌新值 != 原值，
            // 该列不会被重置为未修改，保证 SET/WRITE 与 WHERE 原值的并发检测语义完整
            OptimisticLockHelper.Rotate(entity);

            // 选择性收敛 UPDATE 列：转 Modified 时 EF Core 会把全部属性标记为已修改，
            // 将"当前值 == 原始值"（未被业务修改）且非删除审计列的属性重置为未修改，
            // 最终 UPDATE 仅含删除审计列、并发令牌与业务真改过的列，避免全列写放大，
            // 同时保留"先修改值再删除"的业务变更不丢失；
            // 未轮换令牌的实体（未实现 IOptimisticLock）中，乐观锁列不适用本问题——
            // 并发令牌列即使 IsModified=false 也仍以原值参与 WHERE 判断，不影响并发语义。
            foreach (var property in entry.Properties)
            {
                if (DeletionAuditPropertyNames.Contains(property.Metadata.Name))
                {
                    continue;
                }

                if (Equals(property.CurrentValue, property.OriginalValue))
                {
                    property.IsModified = false;
                }
            }

            return true;
        }

        // 领域显式标记删除：实体保留在集合中、仅 IsDeleted 置 true 的软删除
        if (entry.State == EntityState.Modified && entry.Entity is IDeletion deletion)
        {
            var origin = entry.OriginalValues.GetValue<bool>(nameof(IDeletion.IsDeleted));
            var current = entry.CurrentValues.GetValue<bool>(nameof(IDeletion.IsDeleted));
            if (!origin && current)
            {
                deletion.SetDeletion(userId, userName);

                // 领域显式标记删除同样走"审计策略命中即跳过修改策略"的短路逻辑，
                // 必须在此处轮换令牌，否则软删除 UPDATE 不会触发并发冲突检测
                OptimisticLockHelper.Rotate(deletion);
                return true;
            }
        }

        // 未命中：交由后续策略（如修改审计）处理
        return false;
    }
}
