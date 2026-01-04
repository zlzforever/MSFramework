using System.Collections.Generic;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// Filter 的顺序，越大则先运行
/// </summary>
public static class Constants
{
    /// <summary>
    ///
    /// </summary>
    public const int GlobalException = 0;

    /// <summary>
    ///
    /// </summary>
    public const int ResponseWrapper = 0;

    /// <summary>
    ///
    /// </summary>
    public const int UnitOfWork = 1003;

    /// <summary>
    ///
    /// </summary>
    public const int Audit = 1002;

    /// <summary>
    ///
    /// </summary>
    public static readonly HashSet<string> CommandMethods;

    static Constants()
    {
        CommandMethods = ["POST", "DELETE", "PATCH", "PUT"];
    }
}
