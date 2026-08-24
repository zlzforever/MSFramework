using System;
using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 统一响应包装过滤器，将 Action 返回值包装为 <see cref="ApiResult"/> 格式。
/// 已有 <see cref="ApiResult"/> 返回值不会被二次包装。
/// </summary>
internal sealed class ResponseWrapperFilter(ILogger<ResponseWrapperFilter> logger) : IAsyncResultFilter
{
    /// <summary>
    /// 在 Action 执行完成后包装响应，仅在响应未开始时处理。
    /// </summary>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        logger.LogDebug("开始执行返回结果过滤器");

        // 若是用户自行写入了响应， 不可再次修改
        if (context.HttpContext.Response.HasStarted)
        {
            await next();
            return;
        }

        // 服务调用不做 APIResult 包装
        if (context.HttpContext.Request.Headers.TryGetValue(Defaults.Headers.InternalCall, out var value))
        {
            if ("true".Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }
        }

        // 仅处理 ObjectResult；ProblemDetails 以运行时值为准，避免声明类型掩盖错误响应。
        if (context.Result is ObjectResult objectResult)
        {
            var valueType = objectResult.Value?.GetType() ?? objectResult.DeclaredType;
            // 只要结果是 ApiResult 直接跳过
            if (objectResult.Value is ApiResult ||
                ApiResult.IsApiResult(valueType))
            {
            }
            // 只要结果是 ProblemDetails problemDetails 重新包装一下
            else if (objectResult.Value is ProblemDetails problemDetails)
            {
                objectResult.ContentTypes.Clear();
                var code = objectResult.StatusCode ?? problemDetails.Status ?? StatusCodes.Status200OK;
                objectResult.Value = new ApiResult
                {
                    Success = false, Code = -1, Msg = problemDetails.Title ?? string.Empty, Data = problemDetails
                };
                objectResult.StatusCode = code;
                objectResult.DeclaredType = ApiResult.Type;
            }
            else
            {
                if (valueType is not null && !ApiResult.IsApiResult(valueType))
                {
                    // 以运行时值为准包装普通 ObjectResult；声明类型可能是 ApiResult，不能掩盖实际响应值。
                    var code = objectResult.StatusCode ?? StatusCodes.Status200OK;
                    var success = HttpUtils.IsSuccessStatusCode(code);
                    objectResult.Value = new ApiResult
                    {
                        Success = success, Code = code, Msg = string.Empty, Data = objectResult.Value
                    };
                    objectResult.DeclaredType = ApiResult.Type;
                }
            }
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(ApiResult.Ok);
        }

        await next();

        logger.LogDebug("结束执行返回结果过滤器");
    }
}
