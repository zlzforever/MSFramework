using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Authentication;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MSFramework.Tests;

public class GlobalExceptionFilterTests
{
    /// <summary>
    /// 通过反射实例化 internal 的 GlobalExceptionFilter
    /// </summary>
    private static GlobalExceptionFilterReflection CreateFilter()
    {
        var assembly = typeof(ObjectIdModelBinder).Assembly;
        var filterType = assembly.GetType("MicroserviceFramework.AspNetCore.Filters.GlobalExceptionFilter");
        Assert.NotNull(filterType);

        var logger = Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(filterType));
        var filter = Activator.CreateInstance(filterType, logger);
        var method = filterType.GetMethod("OnException", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        return new GlobalExceptionFilterReflection(filter, method);
    }

    [Fact]
    public void OnException_ReturnsApiResultForFriendlyException()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new InvalidOperationException("最外层包装",
            new InvalidOperationException("中层包装",
                new MicroserviceFrameworkFriendlyException(40001, "业务异常"))));

        filter.Invoke(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(40001, apiResult.Code);
        Assert.Equal("业务异常", apiResult.Msg);
        Assert.Null(apiResult.Data);
        Assert.Empty(result.ContentTypes);
        Assert.False(context.HttpContext.Response.Headers.ContainsKey("X-Correlation-ID"));
    }

    [Fact]
    public void OnException_HidesInternalDetailsInProduction()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new Exception("数据库连接字符串和堆栈不应返回给客户端"));

        filter.Invoke(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(StatusCodes.Status500InternalServerError, apiResult.Code);
        Assert.Equal("系统内部错误", apiResult.Msg);
        Assert.Null(apiResult.Data);
        Assert.DoesNotContain("数据库连接字符串", apiResult.Msg);
        Assert.False(context.HttpContext.Response.Headers.ContainsKey("X-Correlation-ID"));
    }

    [Fact]
    public void OnException_ReturnsGenericApiResultForUnhandledException()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new InvalidOperationException("DI 容器状态错误"));

        filter.Invoke(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(StatusCodes.Status500InternalServerError, apiResult.Code);
        Assert.Equal("系统内部错误", apiResult.Msg);
        Assert.Null(apiResult.Data);
    }

    [Fact]
    public void OnException_UsesGenericApiResultForUnhandledConflictException()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new MicroserviceFrameworkConflictException("资源状态冲突"));

        filter.Invoke(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal(StatusCodes.Status500InternalServerError, apiResult.Code);
        Assert.Equal("系统内部错误", apiResult.Msg);
    }

    [Fact]
    public void OnException_LeavesAlreadyHandledExceptionUntouched()
    {
        var exception = new InvalidOperationException("异常已由其他过滤器处理");
        var context = CreateExceptionContext(exception);
        context.ExceptionHandled = true;

        Assert.Same(exception, context.Exception);

        CreateFilter().Invoke(context);

        Assert.True(context.ExceptionHandled);
        Assert.Null(context.Result);
        Assert.False(context.HttpContext.Response.Headers.ContainsKey("X-Correlation-ID"));
    }

    [Fact]
    public void OnException_UsesFixedMessageForUnauthorizedException()
    {
        const string sensitiveMessage =
            "无法访问 Server=prod-db;Password=secret; /srv/app/appsettings.Production.json";
        var exception = new UnauthorizedAccessException(sensitiveMessage);
        var context = CreateExceptionContext(exception);

        Assert.Same(exception, context.Exception);

        CreateFilter().Invoke(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        Assert.Equal("无权访问", apiResult.Msg);
        Assert.DoesNotContain(sensitiveMessage, apiResult.Msg);
        Assert.DoesNotContain("Server=prod-db", apiResult.Msg);
        Assert.DoesNotContain("/srv/app/appsettings.Production.json", apiResult.Msg);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void OnException_DoesNotGenerateCorrelationIdWhenTraceIdentifierIsBlank(string traceIdentifier)
    {
        var exception = new InvalidOperationException("请求处理失败");
        var context = CreateExceptionContext(exception, traceIdentifier);

        Assert.IsType<InvalidOperationException>(context.Exception);

        CreateFilter().Invoke(context);

        Assert.False(context.HttpContext.Response.Headers.ContainsKey("X-Correlation-ID"));
        Assert.Equal(traceIdentifier, context.HttpContext.TraceIdentifier);
        Assert.True(context.ExceptionHandled);
    }

    [Theory]
    [InlineData("argument", StatusCodes.Status500InternalServerError)]
    [InlineData("authentication", StatusCodes.Status500InternalServerError)]
    [InlineData("unauthorized", StatusCodes.Status403Forbidden)]
    [InlineData("not-found", StatusCodes.Status500InternalServerError)]
    public void OnException_MapsLegacyExceptionStatuses(string exceptionType, int statusCode)
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(exceptionType switch
        {
            "argument" => new ArgumentException("参数不合法"),
            "authentication" => new AuthenticationException("认证失败"),
            "unauthorized" => new UnauthorizedAccessException("无权访问"),
            "not-found" => new KeyNotFoundException("资源不存在"),
            _ => throw new ArgumentOutOfRangeException(nameof(exceptionType))
        });

        filter.Invoke(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(statusCode == StatusCodes.Status403Forbidden
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status500InternalServerError,
            apiResult.Code);
        Assert.Equal(statusCode == StatusCodes.Status403Forbidden ? "无权访问" : "系统内部错误", apiResult.Msg);
        Assert.Null(apiResult.Data);
    }

    /// <summary>
    /// 构建指定根异常的异常过滤器上下文
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>异常上下文</returns>
    private static ExceptionContext CreateExceptionContext(Exception exception, string traceIdentifier = "trace-123")
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = traceIdentifier };
        httpContext.Request.Path = "/test";
        return new ExceptionContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    /// <summary>
    /// 反射调用 GlobalExceptionFilter.OnException 的轻量封装
    /// </summary>
    private sealed class GlobalExceptionFilterReflection(object instance, MethodInfo method)
    {
        /// <summary>
        /// 调用 OnException
        /// </summary>
        /// <param name="context">异常上下文</param>
        public void Invoke(ExceptionContext context)
        {
            method.Invoke(instance, [context]);
        }
    }
}
