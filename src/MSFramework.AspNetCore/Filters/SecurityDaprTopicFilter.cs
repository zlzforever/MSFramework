using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
/// dapr
/// 20X： 消费成功
/// 404： 错误被记录下来，信息被删除
/// other: 警告被记录并重试消息
/// </summary>
public class SecurityDaprTopicFilter : IActionFilter, IOrderedFilter
{
    private static readonly ConcurrentDictionary<MethodInfo, bool> Cache;
    private readonly ILogger<SecurityDaprTopicFilter> _logger;

    private static readonly HashSet<string> InternalIpPrefixes = new()
    {
        "172",
        "10",
        "192",
        "127.0.0.1",
        "localhost",
        "::ffff:127.0.0.1"
    };

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
        var actionDescriptor = ((ControllerActionDescriptor)context.ActionDescriptor);
        var isTopic = Cache.GetOrAdd(actionDescriptor.MethodInfo,
            (entry => context.HasAttribute("Dapr.TopicAttribute")));
        if (!isTopic)
        {
            return;
        }

        var ip = context.HttpContext.GetRemoteIpAddress();
        var isInternalIp = InternalIpPrefixes.Any(x => ip.StartsWith(x));
        if (!string.IsNullOrWhiteSpace(context.HttpContext.Request.Headers["Pubsubname"])
            && !string.IsNullOrWhiteSpace(context.HttpContext.Request.Headers["traceparent"])
            && isInternalIp)
        {
            return;
        }

        context.Result = new OkObjectResult(new ApiResult
        {
            Success = false, Msg = "访问受限", Code = StatusCodes.Status403Forbidden, Data = null
        });
        _logger.LogError($"Executing {actionDescriptor.DisplayName} forbidden");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception == null || context.Exception is MicroserviceFrameworkFriendlyException ||
            context.ExceptionHandled)
        {
            return;
        }

        context.Result = new NotFoundObjectResult(new ApiResult
        {
            Success = false, Msg = "服务器内部错误", Code = context.Exception.HResult, Data = null
        });

        context.ExceptionHandled = true;

        _logger.LogError(context.Exception.ToString());
    }

    public int Order => int.MaxValue;
}
