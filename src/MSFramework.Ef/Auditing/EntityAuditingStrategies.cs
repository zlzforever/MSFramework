using System.Collections.Generic;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MicroserviceFramework.Ef.Auditing;

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

        if (entry.Entity is ICreation entity)
        {
            entity.SetCreation(userId, userName);
        }

        return true;
    }
}

/// <summary>
/// 修改审计策略：处理 <see cref="EntityState.Modified"/> 状态的实体，
/// 对实现了 <see cref="IModification"/> 的实体设置最后修改人、最后修改时间。
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

        return true;
    }
}

/// <summary>
/// 删除审计策略（软删除）：处理 <see cref="EntityState.Deleted"/> 状态的实体。
/// 对实现了 <see cref="IDeletion"/> 的实体直接转 <see cref="EntityState.Modified"/> 并调用
/// <see cref="IDeletion.SetDeletion"/> 写入删除审计字段，
/// 实现软删除而非物理删除。
/// 实体被标记 Deleted 后 EF Core 不会清空属性值，因此无需逐实体 Reload 回填数据；
/// 转 Modified 后 EF Core 会把全部属性标记为已修改，策略将非审计属性重置为未修改，
/// 使最终 UPDATE 仅包含删除审计列，避免全列写放大；乐观锁列保持原值仅参与 WHERE 判断，
/// 并发语义不受影响。
/// </summary>
internal sealed class DeletionAuditingStrategy : IEntityAuditingStrategy
{
    /// <summary>
    /// 删除审计属性名集合：由 <see cref="IDeletion"/> 契约强制定义，
    /// 软删除 UPDATE 仅允许包含这些列（含 <see cref="IDeletion.IsDeleted"/>）。
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
        if (entry.State != EntityState.Deleted)
        {
            return false;
        }

        if (entry.Entity is not IDeletion entity)
        {
            return true; // 非软删除实体，不处理但标记已处理（阻止继续匹配）
        }

        // 转 Modified 实现软删除：实体被标记 Deleted 后属性值仍保留在内存中，
        // 直接转 Modified 并设置删除审计字段即可，省去 Reload 带来的逐行 SELECT。
        entry.State = EntityState.Modified;
        entity.SetDeletion(userId, userName);

        // 收敛 UPDATE 列：转 Modified 时 EF Core 会标记全部属性为已修改，
        // 将删除审计列之外的属性重置为未修改，最终 UPDATE 仅含审计列；
        // 乐观锁列即使 IsModified=false 也仍以原值参与 WHERE 判断，不影响并发语义。
        foreach (var property in entry.Properties)
        {
            if (!DeletionAuditPropertyNames.Contains(property.Metadata.Name))
            {
                property.IsModified = false;
            }
        }

        return true;
    }
}
