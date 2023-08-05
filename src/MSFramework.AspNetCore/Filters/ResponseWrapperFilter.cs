using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

public sealed class ResponseWrapperFilter : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        // if (!context.HttpContext.Request.Headers.Accept.Any(value => SupportedMediaTypes.Contains(value)))
        // {
        //     return;
        // }

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

            var resultType = objectResult.Value.GetType();
            if (resultType == typeof(ApiResult))
            {
                return;
            }

            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ApiResult<>))
            {
                return;
            }

            context.Result = new ObjectResult(
                new ApiResult { Data = objectResult.Value });
        }
    }
}
