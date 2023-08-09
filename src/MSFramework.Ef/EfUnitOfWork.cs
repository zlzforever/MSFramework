using System;
using System.Collections.Generic;
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
    private readonly List<Func<Task>> _tasks;
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
        _tasks = new List<Func<Task>>();
    }

    public void SetAuditingFactory(Func<AuditOperation> auditingFactory)
    {
        Check.NotNull(auditingFactory, nameof(auditingFactory));

        var auditingStores = _serviceProvider.GetServices<IAuditingStore>().ToList();
        if (auditingStores.Count <= 0)
        {
            return;
        }

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

                if (!auditOperation.Entities.Any())
                {
                    return;
                }

                foreach (var auditingStore in auditingStores)
                {
                    auditingStore.AddAsync(sender, auditOperation);
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

        if (_tasks != null)
        {
            foreach (var task in _tasks)
            {
                await task();
            }
        }
    }

    // public void SaveChanges()
    // {
    //     foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
    //     {
    //         dbContext.SaveChanges();
    //     }
    // }

    public void Register(Func<Task> action)
    {
        _tasks.Add(action);
    }
}
