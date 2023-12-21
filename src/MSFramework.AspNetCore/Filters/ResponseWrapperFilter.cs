using System;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

internal sealed class ResponseWrapperFilter(ILogger<ResponseWrapperFilter> logger) : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        logger.LogDebug("开始执行返回结果过滤器");

        // dapr 调用不包装 APIResult
        var invokeFromDapr = !string.IsNullOrEmpty(context.HttpContext.Request.Headers["Pubsubname"])
                             && !string.IsNullOrEmpty(context.HttpContext.Request.Headers["traceparent"]);
        if (invokeFromDapr)
        {
            return;
        }

        // 服务调用不做 APIResult 包装
        if (context.HttpContext.Request.Headers.TryGetValue("service-invocation", out var value))
        {
            if ("true".Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        if (context.Result.GetType() == typeof(ObjectResult))
        {
            var objectResult = (ObjectResult)context.Result;
            context.Result = new ObjectResult(new ApiResult { Data = objectResult.Value });
        }

        if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(ApiResult.Ok);
        }

        logger.LogDebug("执行返回结果过滤器结束");
    }
}
