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
        var apiResult = new ApiResult { Success = false, Code = 42, Msg = "原始响应", Data = "payload" };
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
        var problemDetails = new ProblemDetails { Status = StatusCodes.Status200OK, Title = "问题响应" };
        var context = CreateContext(problemDetails, StatusCodes.Status200OK, ApiResult.Type);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.False(apiResult.Success);
        Assert.Equal(StatusCodes.Status200OK, apiResult.Code);
        Assert.Equal("问题响应", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsProblemDetailsUsingObjectResultStatusCodeAsync()
    {
        var problemDetails = new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "错误" };
        var context = CreateContext(problemDetails, StatusCodes.Status400BadRequest);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(StatusCodes.Status400BadRequest, apiResult.Code);
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
        Assert.Equal(StatusCodes.Status400BadRequest, apiResult.Code);
        Assert.Equal("请求无效", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task WrapsProblemDetailsAsFailureRegardlessOfSuccessfulStatusCodeAsync()
    {
        var problemDetails = new ProblemDetails { Status = StatusCodes.Status200OK, Title = "成功" };
        var context = CreateContext(problemDetails, StatusCodes.Status200OK);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(StatusCodes.Status200OK, apiResult.Code);
        Assert.Equal("成功", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Null(result.DeclaredType);
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
        Assert.Equal(StatusCodes.Status200OK, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Null(result.DeclaredType);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task UsesProblemDetailsStatusWhenObjectResultStatusCodeIsMissingAsync()
    {
        var problemDetails = new ProblemDetails { Status = StatusCodes.Status422UnprocessableEntity, Title = "实体校验失败" };
        var context = CreateContext(problemDetails, null);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        Assert.False(apiResult.Success);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, apiResult.Code);
        Assert.Equal("实体校验失败", apiResult.Msg);
        Assert.Same(problemDetails, apiResult.Data);
        Assert.Null(result.DeclaredType);
        Assert.Empty(result.ContentTypes);
    }

    [Fact]
    public async Task LeavesNullDeclaredTypeResultUnwrappedAsync()
    {
        var context = CreateContext(null, StatusCodes.Status200OK);

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(1, nextCalls);
        Assert.NotNull(result.Value);
        var r = (ApiResult)result.Value;
        Assert.True(r.Success);
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
        Assert.Null(result.DeclaredType);
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
        // 包装后声明类型被清空（以运行时值为准）：SystemTextJsonOutputFormatter 优先按声明类型序列化，
        // 声明类型与被替换后的 ApiResult 运行时值不匹配会抛 InvalidCastException
        Assert.Null(result.DeclaredType);
        Assert.Equal(["application/vnd.api+json"], result.ContentTypes);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(originalValue, apiResult.Data);
        Assert.Equal(
            "{\"success\":true,\"code\":0,\"msg\":\"\",\"data\":\"payload\"}",
            JsonSerializer.Serialize(result.Value));
    }

    /// <summary>
    /// 回归：包装时必须清空陈旧的声明类型。
    /// Action 返回 Task&lt;string&gt;/Task&lt;OrderDto&gt; 等时 ObjectResult.DeclaredType 是声明类型，
    /// 过滤器把 Value 换成 ApiResult 后若保留声明类型，SystemTextJsonOutputFormatter 会按声明类型的
    /// 契约序列化，抛 InvalidCastException（Value 为 ApiResult、声明类型为 string 时实测复现）。
    /// </summary>
    [Fact]
    public async Task ClearsStaleDeclaredTypeWhenWrappingAsync()
    {
        var originalValue = new object();
        var context = CreateContext(originalValue, StatusCodes.Status200OK, typeof(string));

        var nextCalls = await InvokeFilterAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(result.Value);
        Assert.Equal(1, nextCalls);
        Assert.Same(originalValue, apiResult.Data);
        // 清空后由输出格式化器按运行时类型序列化，避免声明类型与运行时值不匹配
        Assert.Null(result.DeclaredType);
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
        // 业务码统一契约：失败时 code 为 HTTP 状态码（与 ProblemDetails 包装、异常过滤器同一口径）
        Assert.Equal(statusCode, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(originalValue, apiResult.Data);
        // 包装后声明类型被清空（以运行时值为准），原因同 WrapsOrdinaryObjectWhenDeclaredTypeIsApiResultAsync
        Assert.Null(result.DeclaredType);
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
        var originalValue = new CustomApiResult<int>(7) { Msg = "自定义结果" };
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

    /// <summary>
    /// 统一包装回归：return Ok(x) 的 OkObjectResult 子类必须与隐式 return 一致地包装
    /// （旧实现按精确类型跳过子类，导致"隐式 return 带壳、return Ok(x) 不带壳"的契约不一致）
    /// </summary>
    [Fact]
    public async Task WrapsOkObjectResultSubclassConsistentlyAsync()
    {
        var originalValue = new object();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new OkObjectResult(originalValue) { DeclaredType = typeof(object) };
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);

        var nextCalls = await InvokeFilterAsync(context);

        var objectResult = Assert.IsType<OkObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(objectResult.Value);
        Assert.Equal(1, nextCalls);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Same(originalValue, apiResult.Data);
        Assert.Null(objectResult.DeclaredType);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    /// <summary>
    /// 统一包装回归：return BadRequest(x) 的 BadRequestObjectResult 子类包装为失败壳，保留 400
    /// </summary>
    [Fact]
    public async Task WrapsBadRequestObjectResultAsFailureAsync()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new BadRequestObjectResult("参数错误");
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);

        await InvokeFilterAsync(context);

        var objectResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(objectResult.Value);
        Assert.False(apiResult.Success);
        // 业务码统一契约：失败时 code 为 HTTP 状态码（400），不再使用业务码 1
        Assert.Equal(StatusCodes.Status400BadRequest, apiResult.Code);
        Assert.Equal("参数错误", apiResult.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    /// <summary>
    /// 统一包装回归：各类型化 ObjectResult 子类与裸 ObjectResult 同契约——
    /// 全部包装为 ApiResult 壳（success 按 2xx 推导、Data 保留原值、DeclaredType 清空），
    /// 且结果实例类型与 HTTP 状态码原样保留
    /// </summary>
    /// <param name="kind">结果类型标识</param>
    /// <param name="expectedStatusCode">期望保留的 HTTP 状态码</param>
    /// <param name="expectedSuccess">期望的 success 推导结果（2xx 为 true）</param>
    [Theory]
    [InlineData("created", StatusCodes.Status201Created, true)]
    [InlineData("createdAtAction", StatusCodes.Status201Created, true)]
    [InlineData("accepted", StatusCodes.Status202Accepted, true)]
    [InlineData("notFound", StatusCodes.Status404NotFound, false)]
    [InlineData("conflict", StatusCodes.Status409Conflict, false)]
    [InlineData("unprocessable", StatusCodes.Status422UnprocessableEntity, false)]
    [InlineData("unauthorized", StatusCodes.Status401Unauthorized, false)]
    public async Task WrapsTypedObjectResultSubclassesConsistentlyAsync(
        string kind, int expectedStatusCode, bool expectedSuccess)
    {
        var originalValue = new object();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        ObjectResult result = kind switch
        {
            "created" => new CreatedResult("/orders/1", originalValue),
            "createdAtAction" => new CreatedAtActionResult("Get", "Order", null, originalValue),
            "accepted" => new AcceptedResult("/orders/1/status", originalValue),
            "notFound" => new NotFoundObjectResult(originalValue),
            "conflict" => new ConflictObjectResult(originalValue),
            "unprocessable" => new UnprocessableEntityObjectResult(originalValue),
            "unauthorized" => new UnauthorizedObjectResult(originalValue),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        result.DeclaredType = typeof(object);
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);

        var nextCalls = await InvokeFilterAsync(context);

        // 实例原样保留（同一引用、类型为原始子类，仅 Value 被包装）
        Assert.Same(result, context.Result);
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(objectResult.Value);
        Assert.Equal(1, nextCalls);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        Assert.Equal(expectedSuccess, apiResult.Success);
        // 非 ProblemDetails 子类：业务码按统一契约（成功 0 / 失败为 HTTP 状态码）
        Assert.Equal(expectedSuccess ? 0 : expectedStatusCode, apiResult.Code);
        Assert.Equal(string.Empty, apiResult.Msg);
        Assert.Same(originalValue, apiResult.Data);
        Assert.Null(objectResult.DeclaredType);
    }

    /// <summary>
    /// 非数据响应保持原始：Content/File/Redirect 不包装、不修改实例与状态
    /// </summary>
    /// <param name="kind">结果类型标识</param>
    [Theory]
    [InlineData("content")]
    [InlineData("file")]
    [InlineData("redirect")]
    public async Task PassesThroughNonDataResultsWithoutWrappingAsync(string kind)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        IActionResult result = kind switch
        {
            "content" => new ContentResult { Content = "plain text", ContentType = "text/plain" },
            "file" => new FileContentResult([1, 2, 3], "application/octet-stream"),
            "redirect" => new RedirectResult("/orders/1"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);

        var nextCalls = await InvokeFilterAsync(context);

        Assert.Equal(1, nextCalls);
        // 实例原样保留（同一引用），未被包装或替换
        Assert.Same(result, context.Result);
    }

    /// <summary>
    /// 统一包装回归：纯 return Ok()（无值 StatusCodeResult）统一为成功壳（与 void Action 行为一致）
    /// </summary>
    [Fact]
    public async Task WrapsPureOkStatusCodeResultAsync()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new OkResult(), null);

        await InvokeFilterAsync(context);

        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(objectResult.Value);
        Assert.True(apiResult.Success);
        Assert.Equal(0, apiResult.Code);
        Assert.Null(apiResult.Data);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    /// <summary>
    /// 统一包装回归：纯 return BadRequest()（无值）包装为失败壳，保留 400
    /// </summary>
    [Fact]
    public async Task WrapsPureBadRequestStatusCodeResultAsFailureAsync()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new BadRequestResult(), null);

        await InvokeFilterAsync(context);

        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var apiResult = Assert.IsType<ApiResult>(objectResult.Value);
        Assert.False(apiResult.Success);
        // 业务码统一契约：失败时 code 为 HTTP 状态码（400），不再使用业务码 1
        Assert.Equal(StatusCodes.Status400BadRequest, apiResult.Code);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    /// <summary>
    /// NoContent（204）例外：HTTP 204 语义禁止响应体，保持原始结果不包装
    /// </summary>
    [Fact]
    public async Task PassesThroughNoContentStatusCodeResultAsync()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new NoContentResult(), null);

        await InvokeFilterAsync(context);

        Assert.IsType<NoContentResult>(context.Result);
    }

    /// <summary>
    /// NoContent（204）例外回归：带 StatusCode=204 的 ObjectResult 同样禁止响应体，
    /// 必须与 NoContentResult 分支保持同一例外（旧实现会把 204 包装出 JSON 壳）
    /// </summary>
    [Fact]
    public async Task PassesThroughObjectResultWithNoContentStatusCodeAsync()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new ObjectResult(new object()) { StatusCode = StatusCodes.Status204NoContent };
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null);

        var nextCalls = await InvokeFilterAsync(context);

        Assert.Equal(1, nextCalls);
        Assert.Same(result, context.Result);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.NotSame(typeof(ApiResult), objectResult.Value?.GetType());
        Assert.Equal(StatusCodes.Status204NoContent, objectResult.StatusCode);
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
