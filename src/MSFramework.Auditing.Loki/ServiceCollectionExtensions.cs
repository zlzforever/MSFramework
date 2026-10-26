using System;
using MicroserviceFramework.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Auditing.Loki;

/// <summary>
///     Loki 审计的依赖注入扩展方法（<b>本包已废弃</b>）
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     添加 Loki 审计日志存储到服务集合。
    ///     <b>本包已废弃</b>：Loki 不便创建索引，审计存储请改用关系数据库
    ///     （如 MSFramework.Ef 的 EfAuditingStore&lt;TDbContext&gt;，推荐 PostgreSQL）或自定义 IAuditingStore
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    [Obsolete("MSFramework.Auditing.Loki 已废弃：审计存储请改用关系数据库（如 EfAuditingStore<TDbContext>，推荐 PostgreSQL）或自定义 IAuditingStore")]
    public static IServiceCollection AddLokiAuditing(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<LokiOptions>>().Value;
            var app = provider.GetRequiredService<ApplicationInfo>();
            return LokiAuditingStore.Create(options, app.Name);
        });
        return services;
    }
}
