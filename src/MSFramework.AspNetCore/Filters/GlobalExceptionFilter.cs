using System;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 全局异常过滤器，将未处理异常统一转换为 <see cref="ApiResult"/> 格式响应。
/// </summary>
internal class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    /// <summary>
    /// 捕获异常并转换为统一响应格式：
    /// <see cref="UnauthorizedAccessException"/> → 403，
    /// <see cref="MicroserviceFrameworkFriendlyException"/> → 200 + 错误信息，
    /// 其他异常 → 500。
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return;
        }

        if (context.Exception is UnauthorizedAccessException uae)
        {
            context.Result =
                new ObjectResult(new ApiResult { Success = false, Msg = uae.Message, Code = 403, Data = null })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

            logger.LogError(context.Exception, "请求 {Method} {Url} 权限异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }
        else if (context.Exception is MicroserviceFrameworkFriendlyException e)
        {
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = e.Message, Code = e.Code, Data = null
            })
            {
                StatusCode = StatusCodes.Status200OK
            };

            logger.LogWarning(context.Exception, "请求 {Method} {Url} 异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }
        else if (FindFriendlyException(context.Exception) is { } friendlyException)
        {
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = friendlyException.Message, Code = friendlyException.Code, Data = null
            })
            {
                StatusCode = StatusCodes.Status200OK
            };

            logger.LogWarning(friendlyException, "请求 {Method} {Url} 异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }
        else
        {
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = "系统内部错误", Code = StatusCodes.Status500InternalServerError, Data = null
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            logger.LogError(context.Exception, "请求 {Method} {Url} 异常", context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        }

        context.ExceptionHandled = true;
    }

    /// <summary>
    /// 遍历异常链查找 <see cref="MicroserviceFrameworkFriendlyException"/>，
    /// 支持任意嵌套深度的 InnerException 包装，找不到返回 null
    /// </summary>
    /// <param name="exception">根异常</param>
    /// <returns>异常链中的友好异常，不存在时返回 null</returns>
    private static MicroserviceFrameworkFriendlyException FindFriendlyException(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is MicroserviceFrameworkFriendlyException friendlyException)
            {
                return friendlyException;
            }
        }

        return null;
    }
}
