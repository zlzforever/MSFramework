using System.Collections.Generic;
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
        foreach (var value in context.HttpContext.Request.Headers.Accept)
        {
            if (!SupportedMediaTypes.Contains(value))
            {
                continue;
            }

            switch (context.Result)
            {
                case EmptyResult:
                    // 根据请求内容的类型，返回对应的空结果
                    // 比如请求的是一个图片，那么返回一个空的图片
                    context.Result = new ObjectResult(ApiResult.Ok);
                    break;
                case ObjectResult { Value: not ApiResult } objResult:
                    context.Result = new ObjectResult(new ApiResult(objResult.Value));
                    break;
            }

            break;
        }
    }
}
