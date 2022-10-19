using System;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

public abstract class ApiControllerBase : ControllerBase, IAsyncActionFilter
{
    protected ISession Session { get; private set; }

    protected ILogger Logger { get; private set; }

    // [NonAction]
    // public virtual ApiResult Success(object data = null, string msg = "")
    // {
    //     return new(data) { Msg = msg };
    // }
    //
    // [NonAction]
    // public virtual ApiResult Error(string msg = null, int code = 1)
    // {
    //     return new(null) { Code = code, Msg = msg };
    // }

    [NonAction]
    public virtual async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        Session = HttpContext.RequestServices.GetService<ISession>();
        Logger = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        var result = await next();

        // 异常会导致 Result 为空
        if (result.Result == null)
        {
            return;
        }

        result.Result = result.Result switch
        {
            // 空内容是使用在 void/Task 这种 Action 中
            EmptyResult => new ObjectResult(ApiResult.Ok),
            _ => result.Result
        };
    }
}
