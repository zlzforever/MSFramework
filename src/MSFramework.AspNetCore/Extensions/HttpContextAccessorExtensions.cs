using System;
using System.Collections.Generic;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace MicroserviceFramework.AspNetCore.Extensions;

public static class HttpContextAccessorExtensions
{
    public static void WriteError(this IHttpContextAccessor accessor, int code, string msg,
        Dictionary<string, IEnumerable<string>> errors = null)
    {
        if (accessor == null)
        {
            throw new ArgumentNullException(nameof(accessor));
        }

        accessor.HttpContext.WriteError(code, msg, errors);
    }

    public static void WriteError(this HttpContext context, int code, string msg,
        Dictionary<string, IEnumerable<string>> errors = null)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Response.StatusCode = 400;
        if (errors == null || errors.Count == 0)
        {
            context.Response.WriteAsJsonAsync(new ApiResult { Code = code, Success = false, Data = null, Msg = msg });
        }
        else
        {

            context.Response.WriteAsJsonAsync(new ApiResultWithErrors
            {
                Code = code,
                Success = false,
                Data = null,
                Msg = msg,
                Errors = errors
            });
        }

        context.Items["WriteError"] = true;
    }
}
