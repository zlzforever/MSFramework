using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

public class ActionExceptionFilter : IActionFilter, IOrderedFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is MicroserviceFrameworkFriendlyException exception)
        {
            context.Result = new BadRequestObjectResult(new ApiResult
            {
                Success = false, Msg = exception.Message, Code = exception.Code, Data = null
            });

            context.ExceptionHandled = true;
        }
    }

    public int Order => Conts.ActionException;
}
