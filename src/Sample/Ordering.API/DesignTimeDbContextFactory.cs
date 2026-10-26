using System;
using System.Data.Common;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ordering.Infrastructure;

namespace Ordering.API;

/// <summary>复用应用配置创建设计时上下文，并在连接数据库前输出脱敏连接信息，供 EF 迁移诊断使用。</summary>
public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<OrderingContext>
{
    /// <summary>构建设计时服务提供程序，在数据库版本探测之前显示安全的连接参数。</summary>
    /// <returns>包含 OrderingContext 注册的服务提供程序，生命周期由设计时工具进程管理。</returns>
    /// <exception cref="InvalidOperationException">数据库配置缺失或连接字符串格式无效。</exception>
    protected override IServiceProvider GetServiceProvider()
    {
        var builder = Program.CreateWebApplicationBuilder(Array.Empty<string>());
        var settings = builder.Configuration.GetDbContextSettings<OrderingContext>();
        Console.WriteLine($"[EF 设计时] OrderingContext，Provider={settings.DatabaseType}，连接串：" +
                          SanitizeConnectionString(settings.ConnectionString));
        return builder.Build().Services;
    }

    /// <summary>仅保留连接目标与身份字段；采用白名单避免密码、证书密码及令牌等其他参数泄露。</summary>
    private static string SanitizeConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "(未配置)";
        }

        var source = new DbConnectionStringBuilder();
        try
        {
            source.ConnectionString = connectionString;
        }
        catch (ArgumentException)
        {
            // 不附带原始解析异常，避免异常消息携带用户提供的敏感连接内容。
            throw new InvalidOperationException("设计时连接字符串格式无效，原始内容已隐藏。");
        }

        var sanitized = new DbConnectionStringBuilder();
        foreach (var key in new[] { "Server", "Host", "Data Source", "Port", "Database", "Initial Catalog",
                     "User ID", "UserID", "Username", "User", "UID", "Integrated Security", "SSL Mode", "SslMode" })
        {
            if (source.TryGetValue(key, out var value))
            {
                sanitized[key] = value;
            }
        }

        sanitized["Password"] = "***";
        return sanitized.ConnectionString;
    }

    /// <summary>为 EF 工具注册控制台日志输出。</summary>
    /// <param name="serviceCollection">设计时服务集合，不允许为空。</param>
    /// <exception cref="ArgumentNullException">服务集合为空。</exception>
    public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        serviceCollection.AddLogging(x => { x.AddConsole(); });
    }
}
