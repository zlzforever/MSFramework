using System.Reflection;

namespace MicroserviceFramework.Ef;

public static class EfRuntimeUtilities
{
    public static readonly bool IsDesignTime;

    static EfRuntimeUtilities()
    {
        IsDesignTime = "ef" == Assembly.GetEntryAssembly()?.GetName().Name?.ToLower();
    }
}
