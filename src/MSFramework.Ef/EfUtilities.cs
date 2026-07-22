using System.Reflection;

namespace MicroserviceFramework.Ef;

/// <summary>
/// EF 核心工具类，提供迁移历史表名和设计时检测
/// </summary>
public static class EfUtilities
{
    /// <summary>
    /// 默认迁移历史记录表名
    /// </summary>
    public const string MigrationsHistoryTable = "___ef_migrations_history";

    /// <summary>
    /// 获取当前是否处于 EF 设计时（迁移/优化）环境
    /// </summary>
    public static readonly bool IsDesignTime;

    static EfUtilities()
    {
        IsDesignTime = "ef" == Assembly.GetEntryAssembly()?.GetName().Name?.ToLower();
    }
}
