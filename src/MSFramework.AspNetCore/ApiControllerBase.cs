﻿using System;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

public abstract class ApiControllerBase : ControllerBase, IAsyncResultFilter, IActionFilter, IAsyncActionFilter
{
    protected ISession Session;

    protected ILogger Logger;

    [NonAction]
    public virtual Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        return next();
    }

    [NonAction]
    public virtual void OnActionExecuting(ActionExecutingContext context)
    {
    }

    [NonAction]
    public virtual void OnActionExecuted(ActionExecutedContext context)
    {
    }

    [NonAction]
    public virtual ApiResult Success(string msg = null, object data = null)
    {
        return new(data)
        {
            Msg = msg
        };
    }

    [NonAction]
    public virtual ApiResult Error(string msg = null, int code = 1)
    {
        return new(null)
        {
            Code = code,
            Msg = msg
        };
    }

    [NonAction]
    public virtual async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        Session = HttpContext.RequestServices.GetService<ISession>();
        Logger = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        OnActionExecuting(context);

        if (context.Result != null)
        {
            return;
        }

        var result = await next();
        OnActionExecuted(result);
    }
}
