using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef;

/// <summary>
///
/// </summary>
public class DbContextFactory
{
    private readonly IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;

    /// <summary>
    ///
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceProvider"></param>
    public DbContextFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        _entityConfigurationTypeFinder = ServiceProvider
            .GetRequiredService<IEntityConfigurationTypeFinder>();
    }

    /// <summary>
    /// 获取指定数据实体的上下文类型
    /// </summary>
    /// <returns>实体所属上下文实例</returns>
    public DbContextBase GetDbContext<TEntity>()
    {
        var dbContextType = _entityConfigurationTypeFinder
            .GetDbContextTypeForEntity(typeof(TEntity));
        return GetDbContext(dbContextType);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="dbContextType"></param>
    /// <returns></returns>
    public DbContextBase GetDbContext(Type dbContextType)
    {
        if (dbContextType == null)
        {
            return null;
        }

        return (DbContextBase)ServiceProvider.GetRequiredService(dbContextType);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public IEnumerable<DbContextBase> GetAllDbContexts()
    {
        foreach (var dbContextType in _entityConfigurationTypeFinder.GetAllDbContextTypes())
        {
            var dbContext = ServiceProvider.GetService(dbContextType);
            if (dbContext != null)
            {
                yield return (DbContextBase)dbContext;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public DbContextBase GetDbContextOrDefault<TEntity>()
    {
        if (!_entityConfigurationTypeFinder.HasDbContextForEntity<TEntity>())
        {
            return null;
        }

        return GetDbContext<TEntity>();
    }
}
