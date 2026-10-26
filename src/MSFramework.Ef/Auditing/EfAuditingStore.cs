using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef.Auditing;

/// <summary>
/// EF Core 审计存储实现，将审计操作持久化到数据库
/// </summary>
/// <typeparam name="TDbContext">审计数据所在 DbContext 类型（须继承 <see cref="DbContextBase"/>）</typeparam>
/// <remarks>
/// <para>
/// 审计与业务<b>彻底隔离</b>：每次落库经独立 scope 解析审计上下文。
/// 旧实现直接注入 scoped 上下文——当宿主用业务上下文类型承载审计时
/// （如 <c>UseEfAuditing&lt;OrderingContext&gt;</c>），审计存储与业务 Handler 共享<b>同一个</b>
/// DbContext 实例，直通保存的是整个 ChangeTracker：可能顺带提交未提交的业务变更
/// （如业务保存失败后的残留 Modified/Added 实体），且绕过业务保存管道
/// （领域事件派发、审计字段应用、审计收集）。独立 scope 保证本次保存的
/// ChangeTracker 只含本次加入的审计实体
/// </para>
/// <para>
/// 审计与业务分属不同上下文、不同保存动作、各自独立提交：
/// 业务 Handler 异常/回滚不会撤销已落库的审计，审计保存失败也不翻转业务结果
/// </para>
/// </remarks>
public class EfAuditingStore<TDbContext>(IServiceScopeFactory scopeFactory) : IAuditingStore
    where TDbContext : DbContextBase
{
    /// <summary>
    /// 异步添加审计操作记录。
    /// 经独立 scope 解析审计上下文（与业务上下文彻底隔离），直通保存——
    /// 上下文中无业务领域事件、审计对象亦被收集管道豁免（<see cref="IAuditObject"/>）
    /// </summary>
    /// <param name="auditOperation">审计操作信息</param>
    public async Task AddAsync(AuditOperation auditOperation)
    {
        // 独立 scope：即使 TDbContext 与业务上下文同类型，也得到一个全新实例，
        // 本次保存只含审计实体，绝不触碰业务的待提交变更
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await dbContext.AddAsync(auditOperation);
        // 直通保存（绕过领域事件派发/审计收集管道）：独立上下文的 ChangeTracker 仅含审计实体
        await dbContext.SaveChangesWithoutPipelineAsync();
    }
}
