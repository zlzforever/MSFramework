using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MicroserviceFramework.Ef.Auditing;

/// <summary>
/// 实体审计策略，封装对不同 <see cref="EntityState"/> 的处理逻辑。
/// 实现类按顺序存入 <see cref="DbContextBase"/> 的策略数组，
/// 遍历时第一个返回 <c>true</c> 的策略即为命中，后续策略不再处理该实体。
/// </summary>
internal interface IEntityAuditingStrategy
{
    /// <summary>
    /// 处理指定的实体条目。返回 <c>true</c> 表示该实体已被处理（发生了状态变更），
    /// 后续策略不再执行；返回 <c>false</c> 表示不匹配，继续尝试下一个策略。
    /// </summary>
    bool Apply(EntityEntry entry, string userId, string userName);
}
