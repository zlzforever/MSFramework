using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    /// <summary>
    /// 所有 DbContext 保存完成后异步调用
    /// </summary>
    public event Func<CancellationToken, ValueTask> SavedChangesAsync;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var contexts = _dbContextFactory.GetAllDbContexts().ToArray();
        if (contexts.Length == 1)
        {
            await contexts[0].SaveChangesAsync(cancellationToken);
        }
        else if (contexts.Length > 1)
        {
            var connection = contexts[0].Database.GetDbConnection();
            if (contexts.Any(context => !ReferenceEquals(connection, context.Database.GetDbConnection())))
            {
                throw new InvalidOperationException(
                    "A unit of work with multiple DbContext instances requires the same database connection. " +
                    "Use an outbox or saga for cross-database consistency.");
            }

            if (contexts.Any(context => context.Database.CurrentTransaction != null))
            {
                throw new InvalidOperationException(
                    "A multi-DbContext unit of work cannot start while a context already has a transaction.");
            }

            using (DbContextBase.DeferDomainEvents())
            await using (var transaction = await contexts[0].Database.BeginTransactionAsync(cancellationToken))
            {
                for (var index = 1; index < contexts.Length; index++)
                {
                    await contexts[index].Database.UseTransactionAsync(
                        transaction.GetDbTransaction(), cancellationToken);
                }

                foreach (var context in contexts)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }

            foreach (var context in contexts)
            {
                await context.DispatchDomainEventsAsync(cancellationToken);
            }
        }

        SavedChanges?.Invoke();

        var callbacks = SavedChangesAsync?.GetInvocationList()
            .Cast<Func<CancellationToken, ValueTask>>()
            .ToArray() ?? [];

        foreach (var callback in callbacks)
        {
            await callback(cancellationToken);
        }
    }

    public void Dispose()
    {
        SavedChanges = null;
        SavedChangesAsync = null;
    }
}
