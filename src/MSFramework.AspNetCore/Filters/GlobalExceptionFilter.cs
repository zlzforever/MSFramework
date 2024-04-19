using System;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

internal class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        logger.LogError(context.Exception, "请求 {Method} {Url} 异常", context.HttpContext.Request.Method,
            context.HttpContext.Request.GetDisplayUrl());

        if (context.ExceptionHandled)
        {
            return;
        }

        if (context.Exception is UnauthorizedAccessException)
        {
            context.HttpContext.Response.StatusCode = 403;
            context.Result = new ObjectResult(new ApiResult { Success = false, Msg = "访问受限", Code = 403, Data = null });
        }
        else if (context.Exception is MicroserviceFrameworkFriendlyException e)
        {
            context.HttpContext.Response.StatusCode = 200;
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = e.Message, Code = e.Code, Data = null
            });
        }
        else
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result =
                new ObjectResult(new ApiResult
                {
                    Success = false, Msg = "系统内部错误", Code = StatusCodes.Status500InternalServerError, Data = null
                });
        }

        context.ExceptionHandled = true;
    }
}
