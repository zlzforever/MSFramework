using System;
using System.Threading.Tasks;
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

        // 若是用户自行写入了响应， 不可再次修改
        if (context.HttpContext.Response.HasStarted)
        {
            await next();
            return;
        }

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
        if (context.Result is ObjectResult objectResult)
        {
            var declaredType = objectResult.DeclaredType ?? objectResult.Value?.GetType();
            if (declaredType == null)
            {
            }
            else if (objectResult.Value is ProblemDetails problemDetails)
            {
                var code = problemDetails.Status ?? 200;
                var success = HttpUtil.IsSuccessStatusCode(code);
                if (success)
                {
                    objectResult.Value = new ApiResult { Data = objectResult.Value, Msg = string.Empty };
                    objectResult.DeclaredType = ApiResult.Type;
                }
                else
                {
                    objectResult.Value = new ApiResult
                    {
                        Success = false, Code = -1, Msg = problemDetails.Title ?? string.Empty
                    };
                    objectResult.StatusCode = code;
                    objectResult.DeclaredType = ApiResult.Type;
                }
            }
            else if (ApiResult.IsApiResult(declaredType))
            {
            }
            else
            {
                objectResult.Value = new ApiResult { Data = objectResult.Value, Msg = string.Empty };
                objectResult.DeclaredType = ApiResult.Type;
            }
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(ApiResult.Ok);
        }

        await next();

        logger.LogDebug("执行返回结果过滤器结束");
    }
}
