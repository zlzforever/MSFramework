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

        switch (result.Result)
        {
            // OkObjectResult、BadRequestObjectResult 都是继承自它，所以需要考虑 StatusCode
            // case ObjectResult or:
            //     // 禁止使用 ApiResult 作为返回值，这样会导致 swagger 不知道返回内容
            //     if (or.Value is not ApiResult)
            //     {
            //         var wrapResult = new ObjectResult(new ApiResult
            //         {
            //             Code = 0,
            //             Success = true,
            //             Data = or.Value,
            //             Msg = "",
            //             Errors = null
            //         }) { StatusCode = or.StatusCode };
            //         result.Result = wrapResult;
            //     }
            //     else
            //     {
            //         Logger.LogWarning(
            //             $"{HttpContext.Request.GetDisplayUrl()} return an ApiResult, this is not suggested");
            //     }
            //
            //     break;
            // 空内容是使用在 void/Task 这种 Action 中
            case EmptyResult:
                result.Result = new ObjectResult(ApiResult.Ok);
                break;
        }
    }
}
