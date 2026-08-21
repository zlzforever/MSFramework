using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using MicroserviceFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 全局异常过滤器，将未处理异常统一转换为 RFC 7807 ProblemDetails 响应。
/// </summary>
internal class GlobalExceptionFilter(
    ILogger<GlobalExceptionFilter> logger,
    IHostEnvironment environment) : IExceptionFilter
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string ProblemDetailsContentType = "application/problem+json";
    private const string GenericErrorDetail = "系统内部错误";

    /// <summary>
    /// 捕获异常并转换为标准 HTTP 错误响应。服务端始终记录完整异常，响应只暴露允许返回的详情。
    /// </summary>
    /// <param name="context">异常过滤器上下文</param>
    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return;
        }

        var exception = context.Exception;
        var friendlyException = FindFriendlyException(exception);
        var (statusCode, title) = GetStatusCode(exception, friendlyException);
        var correlationId = GetCorrelationId(context.HttpContext);
        var problemDetails = new ProblemDetails
        {
            Type = "about:blank",
            Title = title,
            Status = statusCode,
            Detail = GetDetail(exception, statusCode, friendlyException),
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        if (friendlyException != null)
        {
            problemDetails.Extensions["code"] = friendlyException.Code;
        }

        var result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
        result.ContentTypes.Add(ProblemDetailsContentType);
        context.Result = result;
        context.ExceptionHandled = true;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "请求 {Method} {Url} 返回 {StatusCode}，CorrelationId={CorrelationId}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl(),
                statusCode,
                correlationId);
        }
        else
        {
            logger.LogWarning(exception,
                "请求 {Method} {Url} 返回 {StatusCode}，CorrelationId={CorrelationId}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl(),
                statusCode,
                correlationId);
        }
    }

    private string GetDetail(
        Exception exception,
        int statusCode,
        MicroserviceFrameworkFriendlyException friendlyException)
    {
        if (friendlyException != null)
        {
            return friendlyException.Message;
        }

        if (environment.IsDevelopment())
        {
            return exception.Message;
        }

        return statusCode == StatusCodes.Status500InternalServerError
            ? GenericErrorDetail
            : GetPublicDetail(statusCode);
    }

    private static string GetPublicDetail(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "请求参数无效",
            StatusCodes.Status401Unauthorized => "需要身份认证",
            StatusCodes.Status403Forbidden => "无权访问该资源",
            StatusCodes.Status404NotFound => "请求的资源不存在",
            StatusCodes.Status409Conflict => "请求与资源当前状态冲突",
            _ => GenericErrorDetail
        };
    }

    private static (int StatusCode, string Title) GetStatusCode(
        Exception exception,
        MicroserviceFrameworkFriendlyException friendlyException)
    {
        if (friendlyException != null)
        {
            return (StatusCodes.Status400BadRequest, "错误请求");
        }

        return exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "错误请求"),
            AuthenticationException => (StatusCodes.Status401Unauthorized, "未认证"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "禁止访问"),
            KeyNotFoundException or FileNotFoundException =>
                (StatusCodes.Status404NotFound, "资源不存在"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "冲突"),
            _ => (StatusCodes.Status500InternalServerError, "服务器内部错误")
        };
    }

    private static string GetCorrelationId(HttpContext httpContext)
    {
        var correlationId = httpContext.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            httpContext.TraceIdentifier = correlationId;
        }

        httpContext.Response.Headers[CorrelationIdHeader] = correlationId;
        return correlationId;
    }

    /// <summary>
    /// 遍历异常链查找 <see cref="MicroserviceFrameworkFriendlyException"/>，支持任意嵌套深度。
    /// </summary>
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
