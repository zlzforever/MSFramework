using System;
using System.Reflection;

namespace MicroserviceFramework.Ef;

/// <summary>
///
/// </summary>
public static class EfUtilities
{
    /// <summary>
    ///
    /// </summary>
    public const string MigrationsHistoryTable = "___ef_migrations_history";
    /// <summary>
    ///
    /// </summary>
    public static readonly bool IsDesignTime;
    /// <summary>
    ///
    /// </summary>
    public static Type AuditingDbContextType;

    static EfUtilities()
    {
        IsDesignTime = "ef" == Assembly.GetEntryAssembly()?.GetName().Name?.ToLower();
    }
}
