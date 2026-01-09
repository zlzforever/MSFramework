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
        if (context.ExceptionHandled)
        {
            return;
        }

        if (context.Exception is UnauthorizedAccessException uae)
        {
            context.HttpContext.Response.StatusCode = 403;
            context.Result =
                new ObjectResult(new ApiResult { Success = false, Msg = uae.Message, Code = 403, Data = null });

            logger.LogError(context.Exception, "请求 {Method} {Url} 权限异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }
        else if (context.Exception is MicroserviceFrameworkFriendlyException e)
        {
            context.HttpContext.Response.StatusCode = 200;
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = e.Message, Code = e.Code, Data = null
            });

            logger.LogWarning(context.Exception, "请求 {Method} {Url} 异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }
        else if (context.Exception.InnerException is MicroserviceFrameworkFriendlyException innerException)
        {
            context.HttpContext.Response.StatusCode = 200;
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = innerException.Message, Code = innerException.Code, Data = null
            });

            logger.LogWarning(innerException, "请求 {Method} {Url} 异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }
        else
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result =
                new ObjectResult(new ApiResult
                {
                    Success = false, Msg = "系统内部错误", Code = StatusCodes.Status500InternalServerError, Data = null
                });
            logger.LogError(context.Exception, "请求 {Method} {Url} 异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }

        context.ExceptionHandled = true;
    }
}
