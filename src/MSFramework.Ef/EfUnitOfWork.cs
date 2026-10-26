using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 工作单元管理器
/// </summary>
internal class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContextFactory _dbContextFactory;

    /// <summary>
    /// 初始化工作单元管理器
    /// </summary>
    public EfUnitOfWork(DbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// 主（可写）上下文保存完成后调用
    /// </summary>
    public event Func<CancellationToken, ValueTask> SavedChanges;

    /// <summary>
    /// 提交主（可写）上下文的全部待提交变更，提交成功后触发 <see cref="SavedChanges"/> 回调。
    /// <para>
    /// 事务模型约定：一个工作单元中只有一个主上下文——即 <see cref="DbContextFactory.GetAllDbContexts"/>
    /// 返回的上下文中 <see cref="DbContextSettings.IsPrimary"/> 为 true 的第一个；
    /// 均未显式配置时回退为第一个上下文（按实体配置发现顺序注册，兼容旧行为）。业务写入与领域事件
    /// 派发都发生在主上下文上，由其单次 SaveChanges 形成单一隐式事务：领域事件 Handler 抛异常则
    /// 整体不提交。其余上下文只作为只读查询源，不产生待提交变更，也不产生领域事件；
    /// 若在其上意外产生了变更，不会被本方法提交，并随请求结束被丢弃——这是框架约定，不是缺陷。
    /// </para>
    /// <para>
    /// 该约定消除了旧实现的缺陷：旧版对所有上下文逐个 SaveChangesAsync（各上下文各自隐式事务），
    /// 第一个成功、第二个失败时第一个已独立提交，产生部分提交。
    /// 若业务确需跨库写入并要求最终一致，请使用 CAP 本地消息表（Outbox）或 Saga 补偿流程，
    /// 不要把第二个上下文当作可写依赖本方法提交。
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="MicroserviceFrameworkException">有多个上下文被显式标记为主上下文（配置矛盾）时抛出</exception>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var primaryContext = GetPrimaryContext();
        if (primaryContext == null)
        {
            // 无可写上下文：没有保存动作，也不触发 SavedChanges 回调——
            // "没有保存"不应被通知为"保存完成"
            return;
        }

        await primaryContext.SaveChangesAsync(cancellationToken);

        // 提交成功后触发回调（典型用法：领域事件 Handler 在派发时订阅，提交后发布集成事件）
        if (SavedChanges == null)
        {
            return;
        }

        var callbacks = SavedChanges.GetInvocationList()
            .Cast<Func<CancellationToken, ValueTask>>()
            .ToArray();

        foreach (var callback in callbacks)
        {
            // 回调异常分流（设计决策，勿在后续 review 中重复提出）：
            // 执行到此意味着主上下文提交已成功、数据已落库不可撤销；
            // 回调（如 Dapr pubsub 发布集成事件）失败只记告警，不把已成功的提交
            // 翻转成 500——否则客户端按"失败重试"处理会造成重复业务提交。
            // 集成事件按"异步不管"语义由对账/补发兜底；不允许丢失的场景应改用
            // CAP 本地消息表（Outbox）而非依赖本回调。
            try
            {
                await callback(cancellationToken);
            }
            catch (Exception ex)
            {
                Defaults.Logger.LogError(ex,
                    "SavedChanges 回调执行失败（业务数据已提交，集成事件需对账补发）");
            }
        }
    }

    public void Dispose()
    {
        SavedChanges = null;
    }

    /// <summary>
    /// 解析主（可写）上下文：优先取 <see cref="DbContextSettings.IsPrimary"/> 为 true 的第一个；
    /// 均未显式配置时回退为第一个上下文（<see cref="DbContextFactory.GetAllDbContexts"/> = 实体配置发现顺序），
    /// 与 <see cref="Initializer.EntityFrameworkInitializerBase"/> 的迁移目标回退保持同一来源，
    /// 避免多上下文应用中"迁移落到 A 库、业务提交写到 B 库"的 schema 错位。
    /// 多个上下文被同时标记为主上下文属配置矛盾，显式抛出以免提交目标静默错位
    /// </summary>
    /// <returns>主上下文实例；未注册任何上下文时返回 null</returns>
    private DbContextBase GetPrimaryContext()
    {
        var allDbContexts = _dbContextFactory.GetAllDbContexts().ToArray();
        if (allDbContexts.Length == 0)
        {
            return null;
        }

        DbContextBase primaryContext = null;
        foreach (var dbContext in allDbContexts)
        {
            if (dbContext.GetService<DbContextSettings>() is not { IsPrimary: true })
            {
                continue;
            }

            if (primaryContext != null)
            {
                throw new MicroserviceFrameworkException(
                    $"有多个数据库上下文被标记为主（可写）上下文：" +
                    $"{primaryContext.GetType().FullName} 与 {dbContext.GetType().FullName}。" +
                    "主上下文必须唯一，请检查 DbContexts 配置的 IsPrimary");
            }

            primaryContext = dbContext;
        }

        return primaryContext ?? allDbContexts[0];
    }
}
