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

    // public AuditOperation GetAuditOperation()
    // {
    //     return _auditOperation;
    // }

    public void RegisterAuditOperation(AuditOperation auditOperation)
    {
        if (auditOperation == null)
        {
            return;
        }

        foreach (var dbContextBase in _dbContextFactory.GetAllDbContexts())
        {
            dbContextBase.SavingChanges += (sender, _) =>
            {
                if (sender is not DbContextBase db)
                {
                    return;
                }

                var entities = db.GetAuditEntities();
                auditOperation.AddEntities(entities);
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
