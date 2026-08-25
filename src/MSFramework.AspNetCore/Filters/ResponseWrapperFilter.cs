using System;
using System.Reflection;
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
/// 业务码，0 代表成功；非 0 代表业务错误；
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
                logger.LogDebug("结束执行返回结果过滤器");
                return;
            }
        }

        if (context.Result is ObjectResult objectResult)
        {
            // BadRequestObject CreatedObjectResult 之类原样返回
            if (context.Result.GetType() != typeof(ObjectResult))
            {
            }
            else
            {
                var valueType = objectResult.Value?.GetType();
                // ObjectResult 且 Value是ApiResult，直接跳过
                if (valueType != null && (objectResult.Value is ApiResult ||
                                          ApiResult.IsApiResult(valueType)))
                {
                }
                // 只要结果是 ProblemDetails problemDetails 重新包装一下
                else if (objectResult.Value is ProblemDetails problemDetails)
                {
                    objectResult.ContentTypes.Clear();
                    var code = objectResult.StatusCode ?? problemDetails.Status ?? StatusCodes.Status200OK;
                    objectResult.Value = new ApiResult
                    {
                        Success = false,
                        Code = 500,
                        Msg = problemDetails.Title ?? string.Empty,
                        Data = problemDetails
                    };
                    objectResult.StatusCode = code;
                    objectResult.DeclaredType = ApiResult.Type;
                }
                // ObjectResult 且 Value 不是 ApiResult，如是一个 string(Task<string>)
                else
                {
                    var code = objectResult.StatusCode ?? StatusCodes.Status200OK;
                    var success = HttpUtils.IsSuccessStatusCode(code);
                    objectResult.Value = new ApiResult
                    {
                        Success = success, Code = success ? 0 : 1, Msg = string.Empty, Data = objectResult.Value
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
