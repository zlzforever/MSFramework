using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef;

/// <summary>
/// DbContext 工厂，根据实体类型解析所属的 DbContext 实例
/// </summary>
public class DbContextFactory
{
    private readonly IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;

    /// <summary>
    /// 获取服务提供者
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// 初始化 DbContextFactory 实例
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
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
    /// 根据 DbContext 类型获取实例
    /// </summary>
    /// <param name="dbContextType">DbContext 类型</param>
    /// <returns>DbContext 实例，未找到返回 null</returns>
    public DbContextBase GetDbContext(Type dbContextType)
    {
        if (dbContextType == null)
        {
            return null;
        }

        return (DbContextBase)ServiceProvider.GetRequiredService(dbContextType);
    }

    /// <summary>
    /// 获取所有已注册的 DbContext 实例集合
    /// </summary>
    /// <returns>DbContext 实例枚举</returns>
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
    /// 获取指定实体对应的 DbContext 实例，未注册则返回 null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <returns>DbContext 实例，未注册则返回 null</returns>
    public DbContextBase GetDbContextOrDefault<TEntity>()
    {
        if (!_entityConfigurationTypeFinder.HasDbContextForEntity<TEntity>())
        {
            return null;
        }

        return GetDbContext<TEntity>();
    }
}
