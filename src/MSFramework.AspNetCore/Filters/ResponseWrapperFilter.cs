using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

public sealed class ResponseWrapperFilter : ActionFilterAttribute
{
    private static readonly HashSet<string> SupportedMediaTypes = new();

    static ResponseWrapperFilter()
    {
        SupportedMediaTypes.Add("application/json");
        SupportedMediaTypes.Add("text/json");
        SupportedMediaTypes.Add("application/*+json");
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.HttpContext.Request.Headers.Accept.Any(value => SupportedMediaTypes.Contains(value)))
        {
            // 根据请求内容的类型，返回对应的空结果
            // 比如请求的是一个图片，那么返回一个空的图片
            if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(ApiResult.Ok);
            }
            else if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value == null)
                {
                    context.Result = new ObjectResult(ApiResult.Ok);
                    return;
                }

                if (objectResult.Value.GetType().GetGenericTypeDefinition() != typeof(ApiResult<>))
                {
                    context.Result = new ObjectResult(
                        new ApiResult { Data = objectResult.Value });
                }
            }
        }
    }
}
