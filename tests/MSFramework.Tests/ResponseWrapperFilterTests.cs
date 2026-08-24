using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MSFramework.Tests;

public class ResponseWrapperFilterTests
{
    [Fact]
    public async Task WrapsProblemDetailsUsingObjectResultStatusCode()
    {
        var context = CreateContext(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "错误"
        }, StatusCodes.Status400BadRequest);

        await InvokeFilter(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("错误", apiResult.Msg);
    }

    [Fact]
    public async Task WrapsProblemDetailsAsFailureWhenStatusCodeIsMissingFromValue()
    {
        var context = CreateContext(new ProblemDetails { Title = "请求无效" }, StatusCodes.Status400BadRequest);

        await InvokeFilter(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("请求无效", apiResult.Msg);
    }

    private static ResultExecutingContext CreateContext(object value, int? statusCode)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new ObjectResult(value) { StatusCode = statusCode };
        return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);
    }

    private static async Task InvokeFilter(ResultExecutingContext context)
    {
        var assembly = typeof(ObjectIdModelBinder).Assembly;
        var filterType = assembly.GetType("MicroserviceFramework.AspNetCore.Filters.ResponseWrapperFilter");
        Assert.NotNull(filterType);

        var logger = Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(filterType));
        var filter = Activator.CreateInstance(filterType, logger);
        var method = filterType.GetMethod("OnResultExecutionAsync", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        var next = new ResultExecutionDelegate(() =>
        {
            return Task.FromResult(new ResultExecutedContext(context, new List<IFilterMetadata>(), context.Result,
                null));
        });
        var task = (Task)method.Invoke(filter, [context, next]);
        await task;
    }
}
