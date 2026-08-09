using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    private AuditOperation _auditOperation;

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

        // 仅记录当前请求的最新审计操作，SavingChanges 处理器按当前请求解析
        _auditOperation = auditOperation;

        // 每个 DbContext 只订阅一次，避免重复调用 RegisterAuditOperation 时处理器无限累积
        foreach (var dbContextBase in _dbContextFactory.GetAllDbContexts())
        {
            if (_subscribedContexts.Add(dbContextBase))
            {
                dbContextBase.SavingChanges += OnSavingChanges;
            }
        }
    }

    /// <summary>
    /// DbContext 保存前的审计实体收集处理器，按当前请求的 AuditOperation 收集实体
    /// </summary>
    /// <param name="sender">触发保存的 DbContext</param>
    /// <param name="args">事件参数</param>
    private void OnSavingChanges(object sender, EventArgs args)
    {
        if (sender is not DbContextBase db)
        {
            return;
        }

        var auditOperation = _auditOperation;
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
        _auditOperation = null;
        SavedChanges = null;
    }
}
