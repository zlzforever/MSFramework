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
    /// 注意：此处不能释放 scope（<c>using var scope</c>）——<typeparamref name="TDbContext"/> 通常以
    /// Scoped 生命周期注册，释放 scope 会同步释放返回的 DbContext 实例，
    /// 而 EF 设计时工具（<c>dotnet ef</c>）在 <see cref="CreateDbContext"/> 返回后仍会继续使用该上下文
    /// 执行迁移，导致 <see cref="ObjectDisposedException"/>。
    /// scope 生命周期与返回的上下文绑定，设计时命令运行在短生命周期进程中，进程退出即整体回收，
    /// 不存在实际泄漏。
    /// </remarks>
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
