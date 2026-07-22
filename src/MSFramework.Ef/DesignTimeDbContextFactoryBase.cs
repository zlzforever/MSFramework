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
    /// <returns></returns>
    public virtual TDbContext CreateDbContext(string[] args)
    {
        var services = GetServiceProvider();
        return (TDbContext)services.CreateScope()
            .ServiceProvider.GetRequiredService(typeof(TDbContext));
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
