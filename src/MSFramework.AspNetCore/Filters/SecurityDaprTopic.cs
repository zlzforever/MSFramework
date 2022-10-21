using System;
using System.IO;
using MicroserviceFramework.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Filters;

public class SecurityDaprTopic : ActionFilterAttribute
{
    public SecurityDaprTopic()
    {
        Order = int.MaxValue;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Request.EnableBuffering();

        var url = context.HttpContext.Request.GetDisplayUrl();
        var body = context.HttpContext.Request.Body;
        body.Position = 0;
        using var streamReader = new StreamReader(body);
        var text = streamReader.ReadToEnd();
        Console.WriteLine($"{url}, {text}");
        // var configuration= context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
    }
}
