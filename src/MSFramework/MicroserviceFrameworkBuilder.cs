using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework;

/// <summary>
/// MSFramework 构建器，用于配置框架服务和扩展点
/// </summary>
/// <param name="services">服务集合</param>
public class MicroserviceFrameworkBuilder(IServiceCollection services)
{
    /// <summary>
    /// 获取服务集合
    /// </summary>
    public IServiceCollection Services { get; } = services;
}
