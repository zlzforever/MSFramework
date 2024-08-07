using System;
using System.Reflection;

namespace MicroserviceFramework.Ef;

public static class EfUtilities
{
    public const string MigrationsHistoryTable = "___ef_migrations_history";
    public static readonly bool IsDesignTime;
    public static Type AuditingDbContextType;

    static EfUtilities()
    {
        IsDesignTime = "ef" == Assembly.GetEntryAssembly()?.GetName().Name?.ToLower();
    }
}
