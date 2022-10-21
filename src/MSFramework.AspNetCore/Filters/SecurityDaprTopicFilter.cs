using System.Collections.Concurrent;
using System.Reflection;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 在网关层需要把 Pubsubname/traceparent 过滤掉，不允许外部请求带这两个请求头
/// </summary>
public class SecurityDaprTopicFilter : IActionFilter, IOrderedFilter
{
    private static readonly ConcurrentDictionary<MethodInfo, bool> Cache;
    private readonly ILogger<SecurityDaprTopicFilter> _logger;

    static SecurityDaprTopicFilter()
    {
        Cache = new ConcurrentDictionary<MethodInfo, bool>();
    }

    public SecurityDaprTopicFilter(ILogger<SecurityDaprTopicFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var methodInfo = ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo;
        var isTopic = Cache.GetOrAdd(methodInfo, (entry => context.HasAttribute("Dapr.TopicAttribute")));
        if (!isTopic)
        {
            return;
        }

        _logger.LogInformation($"Valid {methodInfo} is called secure");

        if (string.IsNullOrWhiteSpace(context.HttpContext.Request.Headers["Pubsubname"])
            || string.IsNullOrWhiteSpace(context.HttpContext.Request.Headers["traceparent"]))
        {
            context.Result = new BadRequestObjectResult(new ApiResult
            {
                Success = false, Msg = "访问受限", Code = StatusCodes.Status403Forbidden, Data = null
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public int Order => int.MaxValue;
}
