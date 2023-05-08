using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utils;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 工作单元管理器
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContextFactory _dbContextFactory;

    /// <summary>
    /// 初始化工作单元管理器
    /// </summary>
    public EfUnitOfWork(
        DbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public void RegisterAuditing(Func<AuditOperation> auditingFactory)
    {
        Check.NotNull(auditingFactory, nameof(auditingFactory));

        foreach (var dbContextBase in _dbContextFactory.GetAllDbContexts())
        {
            if (dbContextBase.Model.FindEntityType(typeof(AuditOperation)) == null)
            {
                return;
            }

            dbContextBase.SavingChanges += (sender, _) =>
            {
                if (sender is not DbContextBase db)
                {
                    return;
                }

                var auditOperation = auditingFactory();
                var entities = db.GetAuditEntities();
                auditOperation.AddEntities(entities);
                auditOperation.End();
                dbContextBase.Add(auditOperation);
            };
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public void SaveChanges()
    {
        foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
        {
            dbContext.SaveChanges();
        }
    }
}
