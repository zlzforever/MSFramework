using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
/// 限制 Dapr topic action 只能由本机 sidecar 访问。
/// dapr
/// 20X： 消费成功
/// 404： 错误被记录下来，信息被删除
/// other: 警告被记录并重试消息
/// </summary>
public class DaprSecurityMiddleware(RequestDelegate next, ILogger<DaprSecurityMiddleware> logger)
{
    /// <summary>
    /// 执行中间件，验证 Dapr topic 请求的连接来源
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;

        if (endpoint == null)
        {
            await next(context);
            return;
        }

        var isTopic = endpoint.Metadata.Any(x =>
        {
            var type = x.GetType();
            return type.FullName == "Dapr.TopicAttribute" && type.Assembly.GetName().Name == "Dapr.AspNetCore";
        });

        if (!isTopic)
        {
            await next(context);
            return;
        }

        // 安全决策只信任服务器建立连接时记录的地址；未经 Forwarded Headers
        // 中间件校验的 X-Forwarded-For 可由客户端伪造。
        var remoteIpAddress = context.Connection.RemoteIpAddress;
        if (remoteIpAddress != null && IPAddress.IsLoopback(remoteIpAddress))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = 403;
        await context.Response.CompleteAsync();

        logger.LogError("Executing dapr subscribe {Action} forbidden", context.Request.GetDisplayUrl());
    }
}
