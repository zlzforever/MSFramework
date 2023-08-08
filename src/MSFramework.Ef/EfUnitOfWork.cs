using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Auditing;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 工作单元管理器
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContextFactory _dbContextFactory;
    private readonly AuditingOptions _auditingOptions;
    private readonly List<Func<Task>> _tasks;
    private readonly IAuditingStore _auditingStore;


    /// <summary>
    /// 初始化工作单元管理器
    /// </summary>
    public EfUnitOfWork(
        DbContextFactory dbContextFactory, IOptionsMonitor<AuditingOptions> auditingOptions, IAuditingStore auditingStore)
    {
        _dbContextFactory = dbContextFactory;
        _auditingStore = auditingStore;
        _auditingOptions = auditingOptions.CurrentValue;
        _tasks = new List<Func<Task>>();
    }

    public void SetAuditingFactory(Func<AuditOperation> auditingFactory)
    {
        Check.NotNull(auditingFactory, nameof(auditingFactory));

        var auditingDbContext = string.IsNullOrEmpty(_auditingOptions.AuditingDbContextTypeName)
            ? null
            : _dbContextFactory.GetDbContext(Type.GetType(_auditingOptions.AuditingDbContextTypeName));
        var singleAuditOperation = auditingDbContext != null ? auditingFactory() : null;

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

                var auditOperation = singleAuditOperation == null ? auditingFactory() : singleAuditOperation;

                var entities = db.GetAuditEntities();
                auditOperation.AddEntities(entities);
                auditOperation.End();

                if (auditingDbContext == null)
                {
                    dbContextBase.Add(auditOperation);
                }
                else
                {
                    auditingDbContext.Add(auditOperation);
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
