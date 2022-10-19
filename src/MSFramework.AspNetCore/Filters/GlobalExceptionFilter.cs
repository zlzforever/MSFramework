using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is MicroserviceFrameworkFriendlyException e1)
        {
            context.HttpContext.Response.StatusCode = 400;
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = e1.Message, Code = e1.Code, Data = null
            });
        }
        else if (context.Exception.InnerException is MicroserviceFrameworkFriendlyException e2)
        {
            context.HttpContext.Response.StatusCode = 400;
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false, Msg = e2.Message, Code = e2.Code, Data = null
            });
        }
        else
        {
            context.HttpContext.Response.StatusCode = 500;
            context.Result =
                new ObjectResult(new ApiResult { Success = false, Msg = "系统内部错误", Code = 500, Data = null });
        }

        _logger.LogError(context.Exception.ToString());
    }
}
