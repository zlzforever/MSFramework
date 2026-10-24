using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 设计时数据上下文实例工厂基类，用于执行数据迁移
/// </summary>
public abstract class DesignTimeDbContextFactoryBase<TDbContext> :
    IDesignTimeServices,
    IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// 创建一个数据上下文实例
    /// </summary>
    /// <param name="args">参数</param>
    /// <returns>可正常使用的数据上下文实例</returns>
    /// <remarks>
    /// 上下文由 <see cref="IDbContextFactory{TContext}"/> 创建并由 EF 设计时工具释放，
    /// 避免将 scoped 服务作用域泄漏到设计时命令的整个进程生命周期。
    /// </remarks>
    public virtual TDbContext CreateDbContext(string[] args)
    {
        var services = GetServiceProvider();
        var factory = services.GetService<IDbContextFactory<TDbContext>>();
        if (factory == null)
        {
            throw new InvalidOperationException(
                $"Register IDbContextFactory<{typeof(TDbContext).Name}> with AddDbContextFactory or " +
                "AddPooledDbContextFactory before using the design-time factory.");
        }

        return factory.CreateDbContext();
    }

    /// <summary>
    /// 获取服务提供程序，用于解析 DbContext 实例
    /// </summary>
    /// <returns>配置完成的服务提供程序</returns>
    protected abstract IServiceProvider GetServiceProvider();

    /// <summary>
    /// 配置设计时服务，例如注册迁移相关的服务
    /// </summary>
    /// <param name="serviceCollection">服务集合</param>
    public abstract void ConfigureDesignTimeServices(IServiceCollection serviceCollection);
}
