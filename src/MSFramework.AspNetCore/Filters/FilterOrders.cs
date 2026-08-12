using System.Collections.Generic;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// Filter 的执行顺序：Order 越小越先执行（进入阶段按 Order 升序，退出阶段按降序反向执行）
/// </summary>
public static class Constants
{
    /// <summary>
    ///     全局异常过滤器执行顺序
    /// </summary>
    public const int GlobalException = 0;

    /// <summary>
    ///     响应包装过滤器执行顺序
    /// </summary>
    public const int ResponseWrapper = 0;

    /// <summary>
    ///     工作单元过滤器执行顺序
    /// </summary>
    public const int UnitOfWork = 1003;

    /// <summary>
    ///     审计过滤器执行顺序
    /// </summary>
    public const int Audit = 1002;

    /// <summary>
    /// HTTP 写操作方法集合（POST、DELETE、PATCH、PUT）
    /// </summary>
    public static readonly HashSet<string> CommandMethods;

    static Constants()
    {
        CommandMethods = ["POST", "DELETE", "PATCH", "PUT"];
    }
}
