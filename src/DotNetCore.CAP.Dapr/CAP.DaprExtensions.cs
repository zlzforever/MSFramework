using Microsoft.AspNetCore.Builder;

namespace DotNetCore.CAP.Dapr;

/// <summary>
/// CAP Dapr 集成扩展方法
/// </summary>
public static class DaprExtensions
{
    /// <summary>
    /// 配置 Dapr CAP 中间件，映射订阅端点。
    /// <para>
    /// 每个 CAP 订阅会以 <c>app.MapPost(path, ...).WithTopic(pubsub, topic)</c> 映射为
    /// Dapr Topic 投递端点；sidecar 会以 HTTP POST 调用这些端点，必须保证只有 sidecar 能触发：
    /// </para>
    /// <list type="bullet">
    /// <item><description>使用 Dapr 官方 App API Token 方案：daprd 与应用均配置
    /// <c>APP_API_TOKEN</c>，应用侧 <c>AddAuthentication().AddDapr()</c> +
    /// <c>AddAuthorization(o => o.AddDapr())</c>，并在端点上要求认证；</description></item>
    /// <item><description><b>在网关上禁止访问 <c>/dapr/subscribe</c></b>——该端点会列出
    /// 所有订阅的路由路径，不能暴露给外部（如 APISIX 不为该路径创建路由）；</description></item>
    /// <item><description>本方法必须在 CAP 消费者启动前调用（宿主启动时序）。</description></item>
    /// </list>
    /// </summary>
    /// <param name="app">Web 应用程序</param>
    public static void UseDaprCap(this WebApplication app)
    {
        DaprConsumerClientFactory.MapEndpointRoute = (path, pubsub, topic, @delegate) =>
        {
            app.MapPost(path, @delegate).WithTopic(pubsub, topic);
        };
    }
}
