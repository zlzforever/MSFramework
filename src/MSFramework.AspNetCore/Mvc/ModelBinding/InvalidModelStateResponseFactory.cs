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
    public static readonly Func<ActionContext, IActionResult> Instance = context =>
    {
        var errors = context.ModelState.Where(x =>
                x.Value?.ValidationState == ModelValidationState.Invalid)
            .Select(x => new
            {
                Name = x.Key.ToCamelCase(),
                Messages = x.Value?.Errors.Where(z => !string.IsNullOrEmpty(z.ErrorMessage))
                    .Select(y => y.ErrorMessage)
            });

        return new ObjectResult(new ApiResultWithErrors
        {
            Msg = "数据校验不通过", Errors = errors
        })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    };
}
