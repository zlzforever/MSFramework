using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 在网关层需要把 Pubsubname/traceparent 过滤掉，不允许外部请求带这两个请求头
/// dapr
/// 20X： 消费成功
/// 404： 错误被记录下来，信息被删除
/// other: 警告被记录并重试消息
/// </summary>
public class DaprSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DaprSecurityMiddleware> _logger;

    public DaprSecurityMiddleware(RequestDelegate next, ILogger<DaprSecurityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;

        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var isTopic = endpoint.Metadata.Any(x =>
        {
            var type = x.GetType();
            return type.FullName == "Dapr.TopicAttribute" && type.Assembly.GetName().Name == "Dapr.AspNetCore";
        });

        if (!isTopic)
        {
            await _next(context);
            return;
        }

        // 必须带有 Pubsubname/traceparent， 否者必定是非法请求
        if (!string.IsNullOrEmpty(context.Request.Headers["Pubsubname"])
            && !string.IsNullOrEmpty(context.Request.Headers["traceparent"]))
        {
            var ip = context.GetRemoteIpAddress();
            // 仅限内部请求可以访问
            if (ip.IsPrivate())
            {
                await _next(context);
                return;
            }
        }

        context.Response.StatusCode = 403;
        await context.Response.CompleteAsync();

        _logger.LogError("Executing dapr subscribe {Action} forbidden", context.Request.GetDisplayUrl());
    }
}
