using MicroserviceFramework.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    ///
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
    ///
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
