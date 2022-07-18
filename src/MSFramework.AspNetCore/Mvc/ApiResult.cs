using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Mvc;

public class ApiResult : ApiResult<object>
{
    public static readonly ApiResult Ok = new(string.Empty);

    public static readonly ApiResult Error = new(string.Empty)
    {
        Success = false,
        Code = 1,
        Msg = "错误"
    };

    public ApiResult(object value) : base(value)
    {
    }

    public ApiResult(object value, object serializerSettings) : base(value, serializerSettings)
    {
    }
}

public class ApiResult<TData> : ActionResult, IStatusCodeActionResult
{
    private static readonly Type ApiResultType;
    private static readonly Type ActionResultExecutorType;
    private static readonly MethodInfo ExecuteMethodInfo;

    static ApiResult()
    {
        ApiResultType = Type.GetType("Microsoft.AspNetCore.Mvc.JsonResult,Microsoft.AspNetCore.Mvc.Core");
        if (ApiResultType == null)
        {
            throw new MicroserviceFrameworkException("未找到 ApiResult 类型");
        }

        ActionResultExecutorType = typeof(IActionResultExecutor<>).MakeGenericType(ApiResultType);
        ExecuteMethodInfo =
            ActionResultExecutorType.GetMethod("ExecuteAsync", new[] { typeof(ActionContext), ApiResultType });
    }

    /// <summary>
    /// Creates a new <see cref="ApiResult"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to format as JSON.</param>
    public ApiResult(TData value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="ApiResult"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to format as JSON.</param>
    /// <param name="serializerSettings">
    /// The serializer settings to be used by the formatter.
    /// <para>
    /// When using <c>System.Text.Json</c>, this should be an instance of <see cref="JsonSerializerOptions" />.
    /// </para>
    /// <para>
    /// When using <c>Newtonsoft.Json</c>, this should be an instance of <c>JsonSerializerSettings</c>.
    /// </para>
    /// </param>
    public ApiResult(TData value, object serializerSettings)
    {
        Value = value;
        SerializerSettings = serializerSettings;
    }

    /// <summary>
    /// Gets or sets the <see cref="WebRequestMethods.Http.Headers.MediaTypeHeaderValue"/> representing the Content-Type header of the response.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the serializer settings.
    /// <para>
    /// When using <c>System.Text.Json</c>, this should be an instance of <see cref="JsonSerializerOptions" />
    /// </para>
    /// <para>
    /// When using <c>Newtonsoft.Json</c>, this should be an instance of <c>JsonSerializerSettings</c>.
    /// </para>
    /// </summary>
    public object SerializerSettings { get; set; }

    public Dictionary<string, IEnumerable<string>> Errors { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the value to be formatted.
    /// </summary>
    public TData Value { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// 业务代码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Msg { get; set; } = string.Empty;

    /// <inheritdoc />
    public override Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var executor = context.HttpContext.RequestServices.GetRequiredService(ActionResultExecutorType);

        var result = Activator.CreateInstance(ApiResultType, new
        {
            Success,
            Code,
            Msg,
            Errors,
            Data = Value
        });
        if (result == null)
        {
            return (Task)ExecuteMethodInfo.Invoke(executor, new[] { context, result });
        }

        var d = (dynamic)result;
        d.StatusCode = StatusCode;
        d.ContentType = ContentType;
        d.SerializerSettings = SerializerSettings;

        return (Task)ExecuteMethodInfo.Invoke(executor, new[] { context, result });
    }
}
