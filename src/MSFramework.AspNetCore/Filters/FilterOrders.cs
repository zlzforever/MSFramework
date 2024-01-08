using System.Collections.Generic;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// Filter 的顺序，越大则先运行
/// </summary>
public static class Constants
{
    public const int GlobalException = 0;
    public const int ResponseWrapper = 0;
    public const int UnitOfWork = 1002;
    public const int Audit = 1003;

    public static readonly HashSet<string> CommandMethods;

    static Constants()
    {
        CommandMethods = ["POST", "DELETE", "PATCH", "PUT"];
    }
}
