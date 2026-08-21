using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MSFramework.Tests;

public class GlobalExceptionFilterTests
{
    /// <summary>
    /// 通过反射实例化 internal 的 GlobalExceptionFilter
    /// </summary>
    private static GlobalExceptionFilterReflection CreateFilter(string environmentName = null)
    {
        environmentName ??= Environments.Development;
        var assembly = typeof(ObjectIdModelBinder).Assembly;
        var filterType = assembly.GetType("MicroserviceFramework.AspNetCore.Filters.GlobalExceptionFilter");
        Assert.NotNull(filterType);

        var logger = Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(filterType));
        var environment = new TestHostEnvironment(environmentName);
        var filter = Activator.CreateInstance(filterType, logger, environment);
        var method = filterType.GetMethod("OnException", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        return new GlobalExceptionFilterReflection(filter, method);
    }

    [Fact]
    public void OnException_ReturnsProblemDetailsForFriendlyException()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new InvalidOperationException("最外层包装",
            new InvalidOperationException("中层包装",
                new MicroserviceFrameworkFriendlyException(40001, "业务异常"))));

        filter.Invoke(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("业务异常", problemDetails.Detail);
        Assert.Equal(40001, problemDetails.Extensions["code"]);
        Assert.Equal("trace-123", problemDetails.Extensions["correlationId"]);
        Assert.Equal("trace-123", context.HttpContext.Response.Headers["X-Correlation-ID"]);
        Assert.Equal("application/problem+json", result.ContentTypes.Single());
    }

    [Fact]
    public void OnException_HidesInternalDetailsInProduction()
    {
        var filter = CreateFilter(Environments.Production);
        var context = CreateExceptionContext(new Exception("数据库连接字符串和堆栈不应返回给客户端"));

        filter.Invoke(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("系统内部错误", problemDetails.Detail);
        Assert.DoesNotContain("数据库连接字符串", problemDetails.Detail);
        Assert.DoesNotContain("InvalidOperationException", problemDetails.Detail);
        Assert.Equal("trace-123", problemDetails.Extensions["correlationId"]);
    }

    [Fact]
    public void OnException_MapsUnexpectedInvalidOperationToInternalServerError()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new InvalidOperationException("DI 容器状态错误"));

        filter.Invoke(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("服务器内部错误", problemDetails.Title);
        Assert.Equal("DI 容器状态错误", problemDetails.Detail);
    }

    [Fact]
    public void OnException_MapsNestedConflictToConflict()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new InvalidOperationException("外层包装",
            new MicroserviceFrameworkConflictException("资源状态冲突")));

        filter.Invoke(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
    }

    [Theory]
    [InlineData("argument", StatusCodes.Status400BadRequest)]
    [InlineData("authentication", StatusCodes.Status401Unauthorized)]
    [InlineData("unauthorized", StatusCodes.Status403Forbidden)]
    [InlineData("not-found", StatusCodes.Status404NotFound)]
    [InlineData("conflict", StatusCodes.Status409Conflict)]
    public void OnException_MapsKnownExceptionsToStandardStatusCodes(string exceptionType, int statusCode)
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(exceptionType switch
        {
            "argument" => new ArgumentException("参数不合法"),
            "authentication" => new AuthenticationException("认证失败"),
            "unauthorized" => new UnauthorizedAccessException("无权访问"),
            "not-found" => new KeyNotFoundException("资源不存在"),
            "conflict" => new MicroserviceFrameworkConflictException("资源状态冲突"),
            _ => throw new ArgumentOutOfRangeException(nameof(exceptionType))
        });

        filter.Invoke(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(statusCode, problemDetails.Status);
        Assert.Equal("trace-123", problemDetails.Extensions["correlationId"]);
    }

    /// <summary>
    /// 构建指定根异常的异常过滤器上下文
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>异常上下文</returns>
    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-123" };
        httpContext.Request.Path = "/test";
        return new ExceptionContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = typeof(GlobalExceptionFilterTests).Assembly.GetName().Name;

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
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
