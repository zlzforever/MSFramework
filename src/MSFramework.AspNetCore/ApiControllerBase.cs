using MicroserviceFramework.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///     控制器基类，提供 Session 和 Logger 的便捷访问
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    ///     获取当前请求的用户会话信息
    /// </summary>
    protected ISession Session
    {
        get
        {
            field ??= HttpContext.RequestServices.GetRequiredService<ISession>();
            return field!;
        }
    }

    /// <summary>
    ///     获取当前控制器类型的日志记录器
    /// </summary>
    protected ILogger Logger
    {
        get
        {
            field ??= HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            return field!;
        }
    }
}
