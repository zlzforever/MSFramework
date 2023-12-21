using System;
using System.Linq;
using MicroserviceFramework.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

public static class InvalidModelStateResponseFactory
{
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

        return new BadRequestObjectResult(new ApiResultWithErrors
        {
            Code = 1, Success = false, Msg = "数据校验不通过", Errors = errors
        });
    };
}
