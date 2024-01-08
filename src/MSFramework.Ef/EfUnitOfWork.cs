using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 工作单元管理器
/// </summary>
internal class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContextFactory _dbContextFactory;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 初始化工作单元管理器
    /// </summary>
    public EfUnitOfWork(
        DbContextFactory dbContextFactory,
        IServiceProvider serviceProvider)
    {
        _dbContextFactory = dbContextFactory;
        _serviceProvider = serviceProvider;
    }

    public event Func<Task> SavedChanges;

    public void SetAuditOperationFactory(Func<AuditOperation> factory)
    {
        Check.NotNull(factory, nameof(factory));

        var auditingStores = _serviceProvider.GetServices<IAuditingStore>().ToList();
        if (auditingStores.Count == 0)
        {
            return;
        }

        foreach (var dbContextBase in _dbContextFactory.GetAllDbContexts())
        {
            if (dbContextBase.Model.FindEntityType(typeof(AuditOperation)) == null)
            {
                return;
            }

            dbContextBase.SavingChanges += async (sender, _) =>
            {
                if (sender is not DbContextBase db)
                {
                    return;
                }

                // 一定要每次保存都创建新的审计对象
                // 如， 用户自己 save change 一次， 此时 audit operation 已经创建了
                // 再次调用 save change 时， 复用同一个 audit operation， 会导致数据重复, 保存失败
                // TODO: entities 为空时要不要保存？
                var auditOperation = factory();
                var entities = db.GetAuditEntities();
                auditOperation.AddEntities(entities);
                auditOperation.End();

                if (!auditOperation.Entities.Any())
                {
                    return;
                }

                foreach (var auditingStore in auditingStores)
                {
                    await auditingStore.AddAsync(auditOperation);
                    // 不用再单独 commit 了， 和当前请求是同一个 dbcontext
                    // await auditingStore.CommitAsync();
                }
            };
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        SavedChanges?.Invoke().ConfigureAwait(true).GetAwaiter();
    }
}
