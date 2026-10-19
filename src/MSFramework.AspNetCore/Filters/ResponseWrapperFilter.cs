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
            if (objectResult.GetType() != typeof(ObjectResult))
            {
                // BadRequestObjectResult、CreatedObjectResult 子类直接跳过，不包装
            }
            else
            {
                var valueType = objectResult.Value?.GetType();
                // ObjectResult 且 Value是ApiResult，直接跳过
                if (valueType != null && (objectResult.Value is ApiResult ||
                                          ApiResult.IsApiResult(valueType)))
                {
                    // 已经是ApiResult，不处理
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
                    // 包装后 Value 的运行时类型已变为 ApiResult，但 DeclaredType 仍是 Action 的返回类型
                    // （如 Task<string>/Task<OrderDto> 对应的 string/OrderDto）。
                    // SystemTextJsonOutputFormatter 优先按声明类型的契约序列化，
                    // 声明类型与运行时值不匹配时会抛 InvalidCastException（Value 为 ApiResult、声明类型为 string 时实测复现）。
                    // 置 null 表示"以运行时值为准"，与实际序列化对象永远一致；
                    // 不要赋值 ApiResult.Type：它只在包装值恰好是非泛型 ApiResult 时正确，
                    // 若包装值变为 ApiResult<T> 或子类（如带 Errors 属性的 ApiResultWithErrors），
                    // 在按声明类型序列化的行为下会丢失子类属性。
                    objectResult.DeclaredType = null;
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
                    // 包装后 Value 的运行时类型已变为 ApiResult，但 DeclaredType 仍是 Action 的返回类型
                    // （如 Task<string>/Task<OrderDto> 对应的 string/OrderDto）。
                    // SystemTextJsonOutputFormatter 优先按声明类型的契约序列化，
                    // 声明类型与运行时值不匹配时会抛 InvalidCastException（Value 为 ApiResult、声明类型为 string 时实测复现）。
                    // 置 null 表示"以运行时值为准"，与实际序列化对象永远一致；
                    // 不要赋值 ApiResult.Type：它只在包装值恰好是非泛型 ApiResult 时正确，
                    // 若包装值变为 ApiResult<T> 或子类（如带 Errors 属性的 ApiResultWithErrors），
                    // 在按声明类型序列化的行为下会丢失子类属性。
                    objectResult.DeclaredType = null;
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
