using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

public abstract class ApiControllerBase : ControllerBase
{
    private ILogger _logger;
    private ISession _session;

    protected ISession Session
    {
        get
        {
            _session ??= HttpContext.RequestServices.GetRequiredService<ISession>();
            return _session!;
        }
    }

    protected ILogger Logger
    {
        get
        {
            _logger ??= HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            return _logger!;
        }
    }

    // [NonAction]
    // public virtual async Task OnActionExecutionAsync(
    //     ActionExecutingContext context,
    //     ActionExecutionDelegate next)
    // {
    //     if (context == null || next == null)
    //     {
    //         Logger.LogWarning("ActionExecutingContext or ActionExecutionDelegate is null");
    //         return;
    //     }
    //
    //     var actionExecutedContext = await next();
    //
    //     // 通常情况下异常会导致 Result 为空，但添加 ActionExceptionFilter 后，感知到导常后会返回 BadrequestObjectResult
    //     // 是否有其它情况会导致 Result 为空?
    //     if (actionExecutedContext.Result == null)
    //     {
    //         return;
    //     }
    //
    //     actionExecutedContext.Result = actionExecutedContext.Result switch
    //     {
    //         // 空内容是使用在 void/Task 这种 Action 中
    //         EmptyResult => new ObjectResult(ApiResult.Ok),
    //         _ => actionExecutedContext.Result
    //     };
    // }
}
