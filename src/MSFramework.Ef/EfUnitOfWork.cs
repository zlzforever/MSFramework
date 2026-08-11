using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 工作单元管理器
/// </summary>
internal class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContextFactory _dbContextFactory;
    private readonly HashSet<DbContextBase> _subscribedContexts = [];

    /// <summary>
    /// 初始化工作单元管理器
    /// </summary>
    public EfUnitOfWork(DbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// 所有 DbContext 保存完成后调用
    /// </summary>
    public event Action SavedChanges;

    public void RegisterAuditOperation(AuditOperation auditOperation)
    {
        if (auditOperation == null)
        {
            return;
        }

        // 每个 DbContext 只订阅一次，避免重复调用 RegisterAuditOperation 时处理器无限累积；
        // 审计操作本身不再存储，由请求执行流（AuditOperationContext.AsyncLocal）承载，
        // SavingChanges 处理器在自身执行流中读取，从根源消除池化 DbContext 下残留订阅读错对象的跨请求污染
        foreach (var dbContextBase in _dbContextFactory.GetAllDbContexts())
        {
            if (_subscribedContexts.Add(dbContextBase))
            {
                dbContextBase.SavingChanges += OnSavingChanges;
            }
        }
    }

    /// <summary>
    /// DbContext 保存前的审计实体收集处理器，从当前执行流的 <see cref="AuditOperationContext"/> 读取审计操作并收集实体
    /// </summary>
    /// <param name="sender">触发保存的 DbContext</param>
    /// <param name="args">事件参数</param>
    private void OnSavingChanges(object sender, EventArgs args)
    {
        if (sender is not DbContextBase db)
        {
            return;
        }

        // 仅当当前执行流承载审计操作时才收集，避免无审计请求或跨请求残留值被误收集
        var auditOperation = AuditOperationContext.Value;
        if (auditOperation == null)
        {
            return;
        }

        var entities = db.GetAuditEntities();
        auditOperation.AddEntities(entities);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        SavedChanges?.Invoke();
    }

    public void Dispose()
    {
        // 退订所有已订阅 DbContext 的保存事件，避免作用域销毁后处理器继续被调用
        foreach (var dbContext in _subscribedContexts)
        {
            dbContext.SavingChanges -= OnSavingChanges;
        }

        _subscribedContexts.Clear();
        SavedChanges = null;
    }
}
