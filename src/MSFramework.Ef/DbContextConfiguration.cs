using System;

namespace MicroserviceFramework.Ef;

public class DbContextConfiguration
{
    /// <summary>
    /// 初始化一个<see cref="DbContextConfiguration"/>类型的新实例
    /// </summary>
    public DbContextConfiguration()
    {
        AutoMigrationEnabled = false;
        AutoTransactionsEnabled = true;
        EnableSensitiveDataLogging = false;
        UseUnderScoreCase = true;
    }

    /// <summary>
    /// 获取 上下文类型
    /// </summary>
    public Type DbContextType => string.IsNullOrEmpty(DbContextTypeName) ? null : Type.GetType(DbContextTypeName);

    /// <summary>
    /// 获取或设置 上下文类型全名
    /// </summary>
    public string DbContextTypeName { get; set; }

    /// <summary>
    /// 获取或设置 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 批量提交
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// 启用事务
    /// </summary>
    public bool AutoTransactionsEnabled { get; set; }

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
    /// 数据库前缀
    /// </summary>
    public string TablePrefix { get; set; }

    public string MigrationsAssembly { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Schema { get; set; }
}
