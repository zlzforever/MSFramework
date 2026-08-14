using System;
using System.Collections.Generic;
using System.Reflection;
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

        var loggerType = typeof(ILogger<>).MakeGenericType(filterType);
        var logger = Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(filterType));
        var filter = Activator.CreateInstance(filterType, logger);
        var method = filterType.GetMethod("OnException", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        return new GlobalExceptionFilterReflection(filter, method);
    }

    [Fact]
    public void OnException_UnwrapsDeeplyNestedFriendlyException()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new InvalidOperationException("最外层包装",
            new InvalidOperationException("中层包装",
                new MicroserviceFrameworkFriendlyException(40001, "业务异常"))));

        filter.Invoke(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.False(apiResult.Success);
        Assert.Equal("业务异常", apiResult.Msg);
        Assert.Equal(40001, apiResult.Code);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public void OnException_Returns500_WhenNoFriendlyExceptionInChain()
    {
        var filter = CreateFilter();
        var context = CreateExceptionContext(new InvalidOperationException("普通异常"));

        filter.Invoke(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal("系统内部错误", apiResult.Msg);
        Assert.Equal(StatusCodes.Status500InternalServerError, apiResult.Code);
    }

    /// <summary>
    /// 构建指定根异常的异常过滤器上下文
    /// </summary>
    /// <param name="exception">根异常</param>
    /// <returns>异常上下文</returns>
    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        return new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
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
