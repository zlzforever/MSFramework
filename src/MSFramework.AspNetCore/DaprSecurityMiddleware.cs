using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
/// 使用 DAPR_API_TOKEN 环境变量来控制访问 dapr 的权限
/// 然后限制 topic 的  action 只能是内部访问（sidecar)
/// dapr
/// 20X： 消费成功
/// 404： 错误被记录下来，信息被删除
/// other: 警告被记录并重试消息
/// </summary>
public class DaprSecurityMiddleware(RequestDelegate next, ILogger<DaprSecurityMiddleware> logger)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
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

        var ip = context.GetRemoteIpAddress();
        // 仅限内部请求可以访问
        if (IPAddress.IsLoopback(ip))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = 403;
        await context.Response.CompleteAsync();

        logger.LogError("Executing dapr subscribe {Action} forbidden", context.Request.GetDisplayUrl());
    }
}
