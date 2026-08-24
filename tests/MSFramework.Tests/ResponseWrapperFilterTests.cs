using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
    public async Task WrapsProblemDetailsUsingObjectResultStatusCodeAsync()
    {
        var context = CreateContext(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "错误"
        }, StatusCodes.Status400BadRequest);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("错误", apiResult.Msg);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsProblemDetailsAsFailureWhenStatusCodeIsMissingFromValueAsync()
    {
        var context = CreateContext(new ProblemDetails { Title = "请求无效" }, StatusCodes.Status400BadRequest);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("请求无效", apiResult.Msg);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsSuccessfulProblemDetailsAsApiResultAsync()
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status200OK,
            Title = "成功"
        };
        var context = CreateContext(problemDetails, StatusCodes.Status200OK);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.True(apiResult.Success);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Equal(ApiResult.Type, result.DeclaredType);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsProblemDetailsWithApiResultDefaultsWhenStatusAndTitleAreMissingAsync()
    {
        var problemDetails = new ProblemDetails();
        var context = CreateContext(problemDetails, null);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Equal(ApiResult.Type, result.DeclaredType);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task UsesProblemDetailsStatusWhenObjectResultStatusCodeIsMissingAsync()
    {
        var context = CreateContext(new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "实体校验失败"
        }, null);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("实体校验失败", apiResult.Msg);
        Assert.Equal(ApiResult.Type, result.DeclaredType);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task LeavesNullDeclaredTypeResultUnwrappedAsync()
    {
        var context = CreateContext(null, StatusCodes.Status200OK);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.Null(result.Value);
        Assert.Null(result.DeclaredType);
    }

    [Fact]
    public async Task PassesThroughWhenResponseHasStartedAsync()
    {
        var originalValue = new object();
        var context = CreateContext(originalValue, StatusCodes.Status200OK);
        context.HttpContext.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());

        Assert.True(context.HttpContext.Response.HasStarted);

        var nextCalls = await InvokeFilterAsync(context);

        Assert.Equal(1, nextCalls);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Same(originalValue, result.Value);
    }

    [Fact]
    public async Task PassesThroughInternalCallWithoutWrappingAsync()
    {
        var originalValue = new object();
        var context = CreateContext(originalValue, StatusCodes.Status200OK);
        context.HttpContext.Request.Headers[Defaults.Headers.InternalCall] = "true";

        var nextCalls = await InvokeFilterAsync(context);

        Assert.Equal(1, nextCalls);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Same(originalValue, result.Value);
    }

    [Fact]
    public async Task WrapsWhenInternalCallHeaderIsNotTrueAsync()
    {
        var originalValue = new object();
        var context = CreateContext(originalValue, StatusCodes.Status200OK);
        context.HttpContext.Request.Headers[Defaults.Headers.InternalCall] = "false";

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(originalValue, apiResult.Data);
        Assert.Equal(ApiResult.Type, result.DeclaredType);
    }

    private static ResultExecutingContext CreateContext(object value, int? statusCode)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new ObjectResult(value) { StatusCode = statusCode };
        if (value is ProblemDetails)
        {
            result.ContentTypes.Add("application/problem+json");
        }

        return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);
    }

    private static async Task<int> InvokeFilterAsync(ResultExecutingContext context)
    {
        var assembly = typeof(ObjectIdModelBinder).Assembly;
        var filterType = assembly.GetType("MicroserviceFramework.AspNetCore.Filters.ResponseWrapperFilter");
        Assert.NotNull(filterType);

        var logger = Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(filterType));
        var filter = Activator.CreateInstance(filterType, logger);
        var method = filterType.GetMethod("OnResultExecutionAsync", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        var nextCalls = 0;
        var next = new ResultExecutionDelegate(() =>
        {
            nextCalls++;
            return Task.FromResult(new ResultExecutedContext(context, new List<IFilterMetadata>(), context.Result,
                null));
        });
        var task = (Task)method.Invoke(filter, [context, next]);
        await task;
        return nextCalls;
    }

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = Stream.Null;

        public bool HasStarted => true;

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }
    }
}
