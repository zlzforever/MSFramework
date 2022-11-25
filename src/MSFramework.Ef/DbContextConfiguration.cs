using System;

namespace MicroserviceFramework.Ef;

public class DbContextConfiguration
{
    private Type _type;

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
    public Type DbContextType
    {
        get
        {
            if (_type != null)
            {
                return _type;
            }

            _type = string.IsNullOrEmpty(DbContextTypeName) ? null : Type.GetType(DbContextTypeName);
            if (_type == null)
            {
                throw new ArithmeticException($"{DbContextTypeName} is not a valid data context type");
            }

            return _type;
        }
    }

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
}
