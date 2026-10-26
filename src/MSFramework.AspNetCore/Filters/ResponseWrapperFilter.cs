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
/// 业务码统一契约（与 ProblemDetails 包装、模型校验失败、全局异常过滤器同一口径）：
/// <c>success=true</c> 时 <c>code=0</c>；<c>success=false</c>（非 2xx）时
/// <c>code=HTTP 状态码</c>（400/404/409/500…），客户端可凭 code 统一分流；
/// 已有 <see cref="ApiResult"/> 返回值不会被二次包装。
/// <para>
/// 统一包装契约：所有返回数据的 <c>ObjectResult</c>（含 <c>return Ok(x)</c>/<c>BadRequest(x)</c>/
/// <c>NotFound(x)</c>/<c>Created(...)</c> 等子类）一律包装，<c>success</c> 按 HTTP 状态码推导；
/// 纯 <c>Ok()</c>/<c>BadRequest()</c>/<c>NotFound()</c> 等无值状态结果也统一为壳（保留各自状态码，
/// 与 void Action 的 <c>EmptyResult</c> 包装行为一致）；<c>NoContent()</c>（204，HTTP 语义禁止
/// 响应体）与非数据响应（File/Redirect/Content）保持原始。
/// </para>
/// <para>
/// 与 <c>[ApiController]</c> 的交互（端到端实测，勿在后续 review 中重复提出）：
/// 标注 <c>[ApiController]</c> 的控制器自带 <c>ClientErrorResultFilter</c>（Order -2000，
/// 先于本过滤器运行），会把 4xx 无值状态结果先转换为标准 ProblemDetails（含 rfc9110 链接与
/// traceId），因此这类响应最终走 ProblemDetails 分支——code 与 HTTP 状态码一致、msg 为标题；
/// 该行为与环境无关（Development/Production 实测一致），由 <c>[ApiController]</c> 决定而非
/// 环境开关。未标注 <c>[ApiController]</c> 的控制器才会走纯状态结果壳分支（code 同样为 HTTP 状态码）。
/// </para>
/// </summary>
internal sealed class ResponseWrapperFilter(ILogger<ResponseWrapperFilter> logger) : IAsyncResultFilter
{
    /// <summary>
    /// 在 Action 执行完成后包装响应，仅在响应未开始时处理。
    /// </summary>
    /// <param name="context">当前结果上下文，不允许为空。</param>
    /// <param name="next">继续执行结果管道的委托，不允许为空；每次调用本方法仅执行一次。</param>
    /// <returns>包装及后续结果管道执行完成时完成的任务。</returns>
    /// <exception cref="ArgumentNullException">上下文或后续委托为空。</exception>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        logger.LogDebug("开始执行返回结果过滤器");

        // 若是用户自行写入了响应， 不可再次修改
        if (context.HttpContext.Response.HasStarted)
        {
            await next();
            return;
        }

        // Internal-Call 内部调用跳过 ApiResult 包装 —— 设计决策（勿在后续 review 中重复提出）：
        // 该头仅决定响应"格式"（有无 ApiResult 壳），不影响鉴权，也不新增数据暴露——
        // 裸返回的内容与包裹壳内 data 字段的内容完全一致，调用方解析差异仅在壳与字段名。
        // 因此外部伪造该头无安全影响，不作为安全边界处理；内部服务间调用遵循该约定即可。
        if (context.HttpContext.Request.Headers.TryGetValue(Defaults.Headers.InternalCall, out var value) &&
            "true".Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            await next();
            logger.LogDebug("结束执行返回结果过滤器");
            return;
        }

        // 先匹配原样返回的结果，再处理需要包装的结果；File/Redirect/Content 等未匹配类型保持原始。
        switch (context.Result)
        {
            // HTTP 204 禁止响应体，已有 ApiResult（含泛型和派生类）禁止重复包装。
            case ObjectResult { StatusCode: StatusCodes.Status204NoContent }:
            case StatusCodeResult { StatusCode: StatusCodes.Status204NoContent }:
            case ObjectResult { Value: ApiResult }:
                break;
            case ObjectResult { Value: not null } existingResult
                when ApiResult.IsApiResult(existingResult.Value.GetType()):
                break;
            case ObjectResult objectResult:
            {
                if (objectResult.Value is ProblemDetails problemDetails)
                {
                    // ProblemDetails 始终按失败包装，优先保留结果状态码，移除原 ProblemDetails 媒体类型。
                    objectResult.ContentTypes.Clear();
                    var code = objectResult.StatusCode ?? problemDetails.Status ?? StatusCodes.Status200OK;
                    objectResult.Value = new ApiResult
                    {
                        Success = false,
                        Code = code,
                        Msg = problemDetails.Title ?? string.Empty,
                        Data = problemDetails
                    };
                    objectResult.StatusCode = code;
                }
                else
                {
                    // 覆盖全部 ObjectResult 子类，保持隐式返回与 Ok(x)/BadRequest(x) 的包装契约一致。
                    var code = objectResult.StatusCode ?? StatusCodes.Status200OK;
                    var success = HttpUtils.IsSuccessStatusCode(code);
                    objectResult.Value = new ApiResult
                    {
                        Success = success,
                        Code = success ? 0 : code, Msg = string.Empty, Data = objectResult.Value
                    };
                }

                // Value 替换后按运行时类型序列化，避免旧 DeclaredType 导致 InvalidCastException。
                // 不赋 ApiResult.Type，防止未来泛型或带 Errors 的派生类型属性被截掉。
                objectResult.DeclaredType = null;
                break;
            }
            case EmptyResult:
                context.Result = new ObjectResult(ApiResult.Ok);
                break;
            case StatusCodeResult statusCodeResult:
            {
                // 纯状态结果统一为壳，保留 HTTP 状态码；成功业务码为 0，失败为 HTTP 状态码。
                var success = HttpUtils.IsSuccessStatusCode(statusCodeResult.StatusCode);
                context.Result = new ObjectResult(new ApiResult
                {
                    Success = success,
                    Code = success ? 0 : statusCodeResult.StatusCode, Msg = string.Empty, Data = null
                })
                {
                    StatusCode = statusCodeResult.StatusCode
                };
                break;
            }
        }

        await next();

        logger.LogDebug("结束执行返回结果过滤器");
    }
}
