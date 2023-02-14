using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
        if (context.ExceptionHandled)
        {
            return;
        }

        if (context.Exception is MicroserviceFrameworkFriendlyException e)
        {
            context.Result = new BadRequestObjectResult(new ApiResult
            {
                Success = false,
                Msg = e.Message,
                Code = e.Code,
                Data = null
            });
        }
        else
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result =
                new ObjectResult(new ApiResult
                {
                    Success = false,
                    Msg = "系统内部错误",
                    Code = StatusCodes.Status500InternalServerError,
                    Data = null
                });
        }

        context.ExceptionHandled = true;
        _logger.LogError(context.Exception.ToString());
    }
}
