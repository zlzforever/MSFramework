using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef;

/// <summary>
/// DbContext 配置项，从 appsettings.json 的 DbContexts 节点绑定
/// </summary>
// ReSharper disable once UnusedAutoPropertyAccessor.Global
public class DbContextSettings : IDbContextOptionsExtension
{
    /// <summary>
    /// 初始化一个<see cref="DbContextSettings"/>类型的新实例
    /// </summary>
    public DbContextSettings()
    {
        AutoMigrationEnabled = false;
        AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
        EnableSensitiveDataLogging = false;
        UseUnderScoreCase = true;
        Info = new ExtensionInfo(this);
    }

    /// <summary>
    /// 获取或设置 DbContext 类型全名，由配置字典 Key 自动绑定。
    /// </summary>
    public string DbContextTypeName { get; set; }

    /// <summary>
    /// 获取或设置 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 获取或设置 是否启用详细错误信息
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// 批量提交大小
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// 获取或设置 命令超时时间（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 启用事务
    /// </summary>
    public AutoTransactionBehavior AutoTransactionBehavior { get; set; }

    /// <summary>
    /// 获取或设置 是否自动迁移
    /// </summary>
    public bool AutoMigrationEnabled { get; set; }

    /// <summary>
    /// 使用 unix 风格的表名、列名
    /// </summary>
    public bool UseUnderScoreCase { get; set; }

    /// <summary>
    /// 是否开启敏感信息日志
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; }

    /// <summary>
    /// 获取或设置 迁移程序集名称
    /// </summary>
    public string MigrationsAssembly { get; set; }

    // comments: 禁止使用全局 schema 配置
    // 若要查询其他领域的数据，DbContext 中可能会注入只读的模型，使用全局 schema 会
    // 导致 SQL 生成不正确，若要使用 schema 则应该在 ToTable 中自己处理
    // /// <summary>
    // ///
    // /// </summary>
    // public string Schema { get; set; }

    /// <summary>
    /// 数据库前缀
    /// </summary>
    public string TablePrefix { get; set; }

    /// <summary>
    /// 获取或设置 数据库类型（SqlServer/MySql/PostgreSql）
    /// </summary>
    public string DatabaseType { get; set; }

    /// <summary>
    /// 获取或设置 日志缓存时间
    /// </summary>
    public int LoggingCacheTime { get; set; }

    /// <summary>
    /// 获取或设置 是否启用 ServiceProvider 缓存
    /// </summary>
    public bool EnableServiceProviderCaching { get; set; }

    /// <summary>
    /// 获取或设置 是否启用线程安全检查
    /// </summary>
    public bool EnableThreadSafetyChecks { get; set; }

    /// <summary>
    /// 获取或设置 查询拆分行为（SingleQuery/SplitQuery）
    /// </summary>
    public string QuerySplittingBehavior { get; set; }

    /// <summary>
    /// 获取或设置 迁移历史表名
    /// </summary>
    public string MigrationsHistoryTable { get; set; }

    /// <summary>
    /// 获取 DbContext 选项扩展信息
    /// </summary>
    public DbContextOptionsExtensionInfo Info { get; }

    /// <summary>
    /// 使用编译模型
    /// </summary>
    public bool UseCompiledModel { get; set; }

    /// <summary>
    /// 将当前配置注册为单例服务
    /// </summary>
    /// <param name="services">服务集合</param>
    public void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton(this);
    }

    /// <summary>
    /// 验证配置选项
    /// </summary>
    /// <param name="options">DbContext 选项</param>
    public void Validate(IDbContextOptions options)
    {
    }

    private class ExtensionInfo(DbContextSettings extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;
        public override string LogFragment => "Using DbContextSettings";

        public override int GetServiceProviderHashCode()
        {
            return extension.GetHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is ExtensionInfo;
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["DbContextSettings"] = "1";
        }
    }

    /// <summary>
    /// 获取迁移历史表完整名称（含表前缀）
    /// </summary>
    /// <returns>迁移历史表名</returns>
    public string GetMigrationsHistoryTable()
    {
        string migrationsHistoryTable;
        if (!string.IsNullOrWhiteSpace(MigrationsHistoryTable))
        {
            migrationsHistoryTable = $"{TablePrefix}{MigrationsHistoryTable}";
        }
        else
        {
            migrationsHistoryTable = string.IsNullOrWhiteSpace(TablePrefix)
                ? EfUtilities.MigrationsHistoryTable
                : $"{TablePrefix}migrations_history";
        }

        return migrationsHistoryTable;
    }
}
