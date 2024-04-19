using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MicroserviceFramework.AspNetCore.Mvc;
using MicroserviceFramework.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

internal sealed class ResponseWrapperFilter(ILogger<ResponseWrapperFilter> logger) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        logger.LogDebug("开始执行返回结果过滤器");

        // 服务调用不做 APIResult 包装
        if (context.HttpContext.Request.Headers.TryGetValue(Defaults.Headers.InternalCall, out var value))
        {
            if ("true".Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }
        }

        // comments by lewis at 20240103
        // 只能使用 type 比较， 不能使用 is， 不然如 BadRequestObjectResult 也会被二次包装
        if (context.Result.GetType() == typeof(ObjectResult))
        {
            var objectResult = (ObjectResult)context.Result;
            if (objectResult.Value is ProblemDetails problemDetails)
            {
                var code = problemDetails.Status ?? 200;
                var success = HttpUtil.IsSuccessStatusCode(code);
                if (success)
                {
                    context.Result = new ObjectResult(new ApiResult { Data = objectResult.Value, Msg = string.Empty });
                }
                else
                {
                    context.Result = new ObjectResult(new ApiResult
                    {
                        Success = HttpUtil.IsSuccessStatusCode(code),
                        Code = -1,
                        Msg = problemDetails.Title ?? string.Empty
                    }) { StatusCode = code };
                }
            }
            else if (objectResult.Value is ApiResult)
            {
            }
            else
            {
                context.Result = new ObjectResult(new ApiResult { Data = objectResult.Value, Msg = string.Empty });
            }
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(ApiResult.Ok);
        }

        logger.LogDebug("执行返回结果过滤器结束");
        await next();
    }
}
