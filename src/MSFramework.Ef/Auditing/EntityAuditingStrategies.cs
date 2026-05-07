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
/// 对实现了 <see cref="IDeletion"/> 的实体执行 Reload + 转 Modified + SetDeletion，
/// 实现软删除而非物理删除。
/// </summary>
internal sealed class DeletionAuditingStrategy : IEntityAuditingStrategy
{
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

        entry.Reload();
        entry.State = EntityState.Modified;
        entity.SetDeletion(userId, userName);
        return true;
    }
}
