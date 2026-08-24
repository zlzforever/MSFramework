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

        // comments by lewis at 20240103
        // 只能使用 type 比较， 不能使用 is， 不然如 BadRequestObjectResult 也会被二次包装
        if (context.Result is ObjectResult objectResult)
        {
            var declaredType = objectResult.DeclaredType ?? objectResult.Value?.GetType();
            if (declaredType == null)
            {
            }
            else if (objectResult.Value is ProblemDetails problemDetails)
            {
                objectResult.ContentTypes.Clear();
                var code = objectResult.StatusCode ?? problemDetails.Status ?? StatusCodes.Status200OK;
                var success = HttpUtils.IsSuccessStatusCode(code);
                if (success)
                {
                    objectResult.Value = new ApiResult { Data = objectResult.Value, Msg = string.Empty };
                    objectResult.DeclaredType = ApiResult.Type;
                }
                else
                {
                    objectResult.Value = new ApiResult
                    {
                        Success = false,
                        Code = -1,
                        Msg = problemDetails.Title ?? string.Empty
                    };
                    objectResult.StatusCode = code;
                    objectResult.DeclaredType = ApiResult.Type;
                }
            }
            else if (!ApiResult.IsApiResult(declaredType))
            {
                // 根据 ObjectResult 状态码判定 success：
                // 非 2xx（如 BadRequestObjectResult/NotFoundObjectResult/ConflictObjectResult）包装时
                // 必须标记 Success=false，避免 HTTP 状态与 success 字段自相矛盾
                var code = objectResult.StatusCode ?? StatusCodes.Status200OK;
                var success = HttpUtils.IsSuccessStatusCode(code);
                objectResult.Value = new ApiResult
                {
                    Success = success,
                    Code = 0,
                    Msg = string.Empty,
                    Data = objectResult.Value
                };
                objectResult.DeclaredType = ApiResult.Type;
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
