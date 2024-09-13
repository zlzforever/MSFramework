using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef;

/// <summary>
///
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class DbContextSettings : IDbContextOptionsExtension
{
    // private Type _type;

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

    // /// <summary>
    // /// 获取 上下文类型
    // /// </summary>
    // public Type GetDbContextType()
    // {
    //     if (_type != null)
    //     {
    //         return _type;
    //     }
    //
    //     _type = string.IsNullOrEmpty(DbContextTypeName) ? null : Type.GetType(DbContextTypeName);
    //     if (_type == null)
    //     {
    //         throw new ArithmeticException($"找不到类型 {DbContextTypeName}");
    //     }
    //
    //     return _type;
    // }

    // /// <summary>
    // /// 获取或设置 上下文类型全名
    // /// </summary>
    // public string DbContextTypeName { get; set; }

    /// <summary>
    /// 获取或设置 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    ///
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// 批量提交
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    ///
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
    ///
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
    ///
    /// </summary>
    public string DatabaseType { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int LoggingCacheTime { get; set; }

    /// <summary>
    ///
    /// </summary>
    public bool EnableServiceProviderCaching { get; set; }

    /// <summary>
    ///
    /// </summary>
    public bool EnableThreadSafetyChecks { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string QuerySplittingBehavior { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string MigrationsHistoryTable { get; set; }

    /// <summary>
    ///
    /// </summary>
    public DbContextOptionsExtensionInfo Info { get; }

    /// <summary>
    /// 使用编译模型
    /// </summary>
    public bool UseCompiledModel { get; set; } = false;

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    public void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton(this);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
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
    ///
    /// </summary>
    /// <returns></returns>
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
