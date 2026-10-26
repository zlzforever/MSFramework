using System;
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
///     <b>已废弃</b>：请改用 Dapr 官方 App API Token 方案保护 Dapr Topic 端点。
///     <para>
///     官方方案（.NET SDK 内置认证方案，无需自实现）：
///     </para>
///     <list type="number">
///     <item><description>daprd 与应用容器均配置 <c>APP_API_TOKEN</c> 环境变量
///     （K8s：secret <c>app-api-token</c> + 注解 <c>dapr.io/app-token-secret</c>；
///     self-hosted：<c>export APP_API_TOKEN=...</c>）；</description></item>
///     <item><description><c>builder.Services.AddAuthentication().AddDapr()</c>
///     —— 认证方案默认从 <c>APP_API_TOKEN</c> 环境变量读取令牌，校验 sidecar 回调必带的
///     <c>dapr-api-token</c> 头；</description></item>
///     <item><description><c>builder.Services.AddAuthorization(o => o.AddDapr())</c>
///     —— 注册名为 "Dapr" 的授权策略；</description></item>
///     <item><description>Topic 端点标注 <c>[Authorize(Policy = "Dapr")]</c>
///     （或使用全局 fallback policy）。外部探测出路由后直接调用会因缺少令牌被拒绝，
///     与网络位置无关。</description></item>
///     </list>
///     <para>
///     部署要求：<c>/dapr/subscribe</c> 订阅发现端点会列出所有 Topic 路由路径，
///     必须在网关上禁止外部访问（如 APISIX 不为该路径创建路由）；sidecar 从本机访问它即可。
///     </para>
///     <para>
///     在存量宿主显式调用 <c>UseDaprSecurity()</c> 期间，本中间件仍作为网络层纵深防御生效：
///     仅放行 loopback 来源的 Topic 回调（sidecar 与应用同机/同 pod），非 loopback 的
///     Topic 请求直接 403。
///     </para>
///     <para>
///     已知限制（设计决策，勿在后续 review 中重复提出）：
///     来源判定经由 <c>GetRemoteIpAddress()</c>，其会优先读取 <c>X-Forwarded-For</c> 请求头，
///     该头可被任意客户端伪造为 loopback 值从而绕过本校验（fail-open）。
///     这是接受存量实现的已知风险：本中间件已 <see cref="ObsoleteAttribute"/>，
///     正确的安全方案是 Dapr 官方 App API Token（sidecar↔应用共享密钥头校验，见 Obsolete 标注链接），
///     应迁移官方方案而非修补本中间件；如确需收紧，可在网关剥离 <c>X-Forwarded-For</c> 后再转发。
///     </para>
/// </summary>
[Obsolete("改用 Dapr 官方 App API Token 方案：AddAuthentication().AddDapr() + AddAuthorization(o => o.AddDapr()) + [Authorize(Policy = \"Dapr\")]，见 https://docs.dapr.io/operations/security/app-api-token/")]
public class DaprSecurityMiddleware(RequestDelegate next, ILogger<DaprSecurityMiddleware> logger)
{
    /// <summary>
    ///     执行中间件，按网络位置（loopback）限制 Dapr Topic 回调来源
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

        var ip = context.GetRemoteIpAddress();
        // 仅限内部请求可以访问；IP 为 null（无连接地址/转发头无法解析）时视为拒绝，
        // 避免对 null 调用 IPAddress.IsLoopback 抛 ArgumentNullException
        if (ip != null && IPAddress.IsLoopback(ip))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = 403;
        await context.Response.CompleteAsync();

        logger.LogError("Executing dapr subscribe {Action} forbidden", context.Request.GetDisplayUrl());
    }
}
