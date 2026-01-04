using System;
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

    public AuditOperation GetAuditOperation()
    {
        return _auditOperation;
    }

    public void SetAuditOperation(AuditOperation auditOperation)
    {
        if (auditOperation == null)
        {
            return;
        }

        _auditOperation = auditOperation;
        foreach (var dbContextBase in _dbContextFactory.GetAllDbContexts())
        {
            // 对于不同 DbContext，应该把其下面的 Entity 归到对应的审计日志下面
            dbContextBase.SavingChanges += (sender, _) =>
            {
                if (sender is not DbContextBase db)
                {
                    return;
                }

                var entities = db.GetAuditEntities();
                auditOperation.AddEntities(entities);
                auditOperation.End();
            };
        }
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
        SavedChanges = null;
    }
}
