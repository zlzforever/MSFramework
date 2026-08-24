using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
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
    public async Task LeavesNonGenericApiResultUnwrappedWhenDeclaredTypeIsObjectAsync()
    {
        var apiResult = new ApiResult
        {
            Success = false,
            Code = 42,
            Msg = "原始响应",
            Data = "payload"
        };
        var context = CreateContext(apiResult, StatusCodes.Status418ImATeapot, typeof(object));
        Assert.IsType<ObjectResult>(context.Result).ContentTypes.Add("application/vnd.api+json");

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.Same(apiResult, result.Value);
        Assert.Equal(typeof(object), result.DeclaredType);
        Assert.Equal(StatusCodes.Status418ImATeapot, result.StatusCode);
        Assert.Equal(["application/vnd.api+json"], result.ContentTypes);
        Assert.False(apiResult.Success);
        Assert.Equal(42, apiResult.Code);
        Assert.Equal("原始响应", apiResult.Msg);
        Assert.Equal("payload", apiResult.Data);
    }

    [Fact]
    public async Task LeavesGenericApiResultUnwrappedWhenDeclaredTypeIsObjectAsync()
    {
        var apiResult = new ApiResult<int>(7896);
        var context = CreateContext(apiResult, StatusCodes.Status202Accepted, typeof(object));
        Assert.IsType<ObjectResult>(context.Result).ContentTypes.Add("application/vnd.api+json");

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.Same(apiResult, result.Value);
        Assert.Equal(typeof(object), result.DeclaredType);
        Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
        Assert.Equal(["application/vnd.api+json"], result.ContentTypes);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Equal(7896, apiResult.Data);
    }

    [Fact]
    public async Task WrapsProblemDetailsEvenWhenDeclaredTypeIsApiResultAsync()
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status200OK,
            Title = "问题响应"
        };
        var context = CreateContext(problemDetails, StatusCodes.Status200OK, ApiResult.Type);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("问题响应", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsProblemDetailsUsingObjectResultStatusCodeAsync()
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "错误"
        };
        var context = CreateContext(problemDetails, StatusCodes.Status400BadRequest);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("错误", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsProblemDetailsAsFailureWhenStatusCodeIsMissingFromValueAsync()
    {
        var problemDetails = new ProblemDetails { Title = "请求无效" };
        var context = CreateContext(problemDetails, StatusCodes.Status400BadRequest);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("请求无效", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsProblemDetailsAsFailureRegardlessOfSuccessfulStatusCodeAsync()
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
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("成功", apiResult.Msg);
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
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Equal(ApiResult.Type, result.DeclaredType);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task UsesProblemDetailsStatusWhenObjectResultStatusCodeIsMissingAsync()
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "实体校验失败"
        };
        var context = CreateContext(problemDetails, null);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(-1, apiResult.Code);
        Assert.Equal("实体校验失败", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
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

    [Fact]
    public async Task WrapsOrdinaryObjectWhenDeclaredTypeIsApiResultAsync()
    {
        var originalValue = "payload";
        var context = CreateContext(originalValue, StatusCodes.Status200OK, ApiResult.Type);
        Assert.IsType<ObjectResult>(context.Result).ContentTypes.Add("application/vnd.api+json");

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(ApiResult.Type, result.DeclaredType);
        Assert.Equal(["application/vnd.api+json"], result.ContentTypes);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(originalValue, apiResult.Data);
        Assert.Equal(
            "{\"success\":true,\"code\":0,\"msg\":\"\",\"data\":\"payload\"}",
            JsonSerializer.Serialize(result.Value));
    }

    [Fact]
    public async Task PassesThroughNonGenericApiResultAsync()
    {
        var originalValue = new ApiResult { Data = 7, Msg = "原始结果" };
        var context = CreateContext(originalValue, StatusCodes.Status200OK);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.Same(originalValue, result.Value);
        Assert.Null(result.DeclaredType);
    }

    [Fact]
    public async Task PassesThroughGenericApiResultAsync()
    {
        var originalValue = new ApiResult<int>(7) { Msg = "原始结果" };
        var context = CreateContext(originalValue, StatusCodes.Status200OK);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.Same(originalValue, result.Value);
        Assert.Null(result.DeclaredType);
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status404NotFound)]
    [InlineData(StatusCodes.Status500InternalServerError)]
    public async Task WrapsOrdinaryObjectAsFailureForNonSuccessStatusCodesAsync(int statusCode)
    {
        var originalValue = new object();
        var context = CreateContext(originalValue, statusCode);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(originalValue, apiResult.Data);
        Assert.Equal(ApiResult.Type, result.DeclaredType);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("TrUe")]
    public async Task PassesThroughInternalCallHeaderCaseInsensitivelyAsync(string headerValue)
    {
        var originalValue = new object();
        var context = CreateContext(originalValue, StatusCodes.Status200OK);
        context.HttpContext.Request.Headers[Defaults.Headers.InternalCall] = headerValue;

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.Same(originalValue, result.Value);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("1")]
    [InlineData(" true ")]
    public async Task WrapsInternalCallHeaderWhenValueIsNotTrueAsync(string headerValue)
    {
        var originalValue = new object();
        var context = CreateContext(originalValue, StatusCodes.Status200OK);
        context.HttpContext.Request.Headers[Defaults.Headers.InternalCall] = headerValue;

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.True(apiResult.Success);
        Assert.Same(originalValue, apiResult.Data);
    }

    [Fact]
    public async Task WrapsEmptyResultAsSuccessfulApiResultAsync()
    {
        var context = CreateContext(new object(), StatusCodes.Status200OK);
        context.Result = new EmptyResult();

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Null(apiResult.Data);
    }

    [Fact]
    public async Task PropagatesExceptionThrownByNextAsync()
    {
        var context = CreateContext(new object(), StatusCodes.Status200OK);
        var expected = new InvalidOperationException("next failed");
        var nextCalls = 0;
        var next = new ResultExecutionDelegate(() =>
        {
            nextCalls++;
            return Task.FromException<ResultExecutedContext>(expected);
        });

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeFilterAsync(context, next));

        Assert.Same(expected, actual);
        Assert.Equal(1, nextCalls);
    }

    [Fact]
    public async Task PassesThroughCustomGenericApiResultSubclassAsync()
    {
        var originalValue = new CustomApiResult<int>(7)
        {
            Msg = "自定义结果"
        };
        var context = CreateContext(originalValue, StatusCodes.Status201Created);
        var originalResult = Assert.IsType<ObjectResult>(context.Result);
        originalResult.ContentTypes.Add("application/custom+json");
        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.Same(originalValue, result.Value);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Null(result.DeclaredType);
        Assert.Single(result.ContentTypes);
        Assert.Equal("application/custom+json", result.ContentTypes[0]);
        Assert.Equal(
            """{"success":true,"code":0,"msg":"\u81EA\u5B9A\u4E49\u7ED3\u679C","data":7}""",
            JsonSerializer.Serialize((CustomApiResult<int>)result.Value));
    }

    private static ResultExecutingContext CreateContext(object value, int? statusCode, Type declaredType = null)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new ObjectResult(value) { DeclaredType = declaredType, StatusCode = statusCode };
        if (value is ProblemDetails)
        {
            result.ContentTypes.Add("application/problem+json");
        }

        return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);
    }

    private static async Task<int> InvokeFilterAsync(ResultExecutingContext context,
        ResultExecutionDelegate next = null)
    {
        var assembly = typeof(ObjectIdModelBinder).Assembly;
        var filterType = assembly.GetType("MicroserviceFramework.AspNetCore.Filters.ResponseWrapperFilter");
        Assert.NotNull(filterType);

        var logger = Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(filterType));
        var filter = Activator.CreateInstance(filterType, logger);
        var method = filterType.GetMethod("OnResultExecutionAsync", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        var nextCalls = 0;
        next ??= new ResultExecutionDelegate(() =>
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

    private sealed class CustomApiResult<T>(T data) : ApiResult<T>(data);
}
