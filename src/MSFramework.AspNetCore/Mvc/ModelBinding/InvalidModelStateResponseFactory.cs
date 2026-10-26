using System;
using System.Linq;
using MicroserviceFramework.Common;
using MicroserviceFramework.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// 模型验证失败响应工厂，返回统一格式的错误响应
/// </summary>
public static class InvalidModelStateResponseFactory
{
    /// <summary>
    /// 模型验证失败的默认响应委托
    /// </summary>
    /// <remarks>
    /// 错误条目不静默丢弃：仅含 <see cref="Microsoft.AspNetCore.Mvc.ModelBinding.ModelError.Exception"/>
    /// 而无 ErrorMessage 的绑定失败（如值解析异常）回退到异常类型名占位——
    /// 旧实现直接过滤掉该类条目，前端看到"数据校验不通过"却没有任何具体字段错误
    /// </remarks>
    public static readonly Func<ActionContext, IActionResult> Instance = context =>
    {
        var errors = context.ModelState.Where(x =>
                x.Value?.ValidationState == ModelValidationState.Invalid)
            .Select(x => new
            {
                Name = x.Key.ToCamelCase(),
                Messages = x.Value?.Errors.Select(z =>
                    !string.IsNullOrEmpty(z.ErrorMessage)
                        ? z.ErrorMessage
                        // ErrorMessage 为空时回退到异常类型名（如 FormatException），保证错误条目不静默丢失
                        : z.Exception?.GetType().Name ?? "绑定失败")
            });

        return new ObjectResult(new ApiResultWithErrors
        {
            Msg = "数据校验不通过",
            // 业务码统一契约：失败时的 code 为 HTTP 状态码（与 ProblemDetails 包装、
            // 全局异常过滤器同一口径），不再使用默认业务码 1
            Code = StatusCodes.Status400BadRequest,
            Errors = errors
        })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    };
}
