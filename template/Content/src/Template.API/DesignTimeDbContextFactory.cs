using MicroserviceFramework.Ef;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure;

namespace Template.API;

/// <summary>复用运行时配置创建迁移上下文，依赖 User Secrets 或环境变量，不执行 HTTP 启动管道。</summary>
public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<TemplateDbContext>
{
    /// <summary>构建设计时容器，生命周期交给 EF 工具进程。</summary>
    /// <returns>包含数据库设置和实体配置发现器的服务提供程序。</returns>
    /// <exception cref="InvalidOperationException">运行时必需配置缺失。</exception>
    protected override IServiceProvider GetServiceProvider() =>
        Program.CreateWebApplicationBuilder([]).Build().Services;

    /// <summary>保持 Provider 默认设计时服务，不额外替换迁移生成器。</summary>
    /// <param name="serviceCollection">EF 设计时服务集合，不允许为空。</param>
    /// <exception cref="ArgumentNullException">服务集合为空。</exception>
    public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection) =>
        ArgumentNullException.ThrowIfNull(serviceCollection);
}
