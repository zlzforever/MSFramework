using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
/// 限制 Dapr topic action 只能由本机 sidecar 访问。
/// dapr
/// 20X： 消费成功
/// 404： 错误被记录下来，信息被删除
/// other: 警告被记录并重试消息
/// </summary>
public class DaprSecurityMiddleware(
    RequestDelegate next,
    ILogger<DaprSecurityMiddleware> logger,
    IConfiguration configuration)
{
    private const string TokenHeader = "dapr-app-token";

    /// <summary>
    /// 执行中间件，验证 Dapr topic 请求的连接来源
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;

        if (endpoint == null)
        {
            await RejectAsync(context, StatusCodes.Status403Forbidden,
                "Dapr security middleware must run after endpoint routing");
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

        var expectedToken = configuration["DAPR_APP_TOKEN"];
        var suppliedToken = context.Request.Headers[TokenHeader].ToString();
        if (!TokenMatches(expectedToken, suppliedToken))
        {
            await RejectAsync(context, StatusCodes.Status401Unauthorized,
                "Dapr topic request has an invalid application token");
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

        await RejectAsync(context, StatusCodes.Status403Forbidden,
            "Dapr topic request did not originate from the local sidecar");
    }

    private static bool TokenMatches(string expectedToken, string suppliedToken)
    {
        if (string.IsNullOrWhiteSpace(expectedToken) || string.IsNullOrEmpty(suppliedToken))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedToken);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedToken);
        return expectedBytes.Length == suppliedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    private async Task RejectAsync(HttpContext context, int statusCode, string reason)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.CompleteAsync();
        logger.LogWarning("{Reason}: {Action}", reason, context.Request.GetDisplayUrl());
    }
}
