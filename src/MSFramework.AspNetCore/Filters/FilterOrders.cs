using System.Collections.Generic;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// Filter 的顺序，越大则先运行
/// </summary>
public static class Conts
{
    public const int UnitOfWork = 1000;
    public const int Audit = 2000;
    public const int ActionException = int.MaxValue - 10;

    public static readonly HashSet<string> CommandMethods;

    static Conts()
    {
        CommandMethods = new HashSet<string> { "POST", "DELETE", "PATCH", "PUT" };
    }
}
