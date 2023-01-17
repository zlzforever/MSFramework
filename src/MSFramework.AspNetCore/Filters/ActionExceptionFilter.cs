using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

public class ActionExceptionFilter : IActionFilter, IOrderedFilter
{
    private readonly ILogger<ActionExceptionFilter> _logger;

    public ActionExceptionFilter(ILogger<ActionExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // 仅处理 MicroserviceFrameworkFriendlyException
        if (context.Exception == null
            || context.ExceptionHandled
            || context.Exception is not MicroserviceFrameworkFriendlyException exception)
        {
            return;
        }

        context.Result = new BadRequestObjectResult(new ApiResult
        {
            Success = false, Msg = exception.Message, Code = exception.Code, Data = null
        });

        context.ExceptionHandled = true;
        _logger.LogError(exception.Message);
    }

    public int Order => Constants.ActionException;
}
