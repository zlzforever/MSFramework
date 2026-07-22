using Microsoft.AspNetCore.Builder;

namespace DotNetCore.CAP.Dapr;

/// <summary>
/// CAP Dapr 集成扩展方法
/// </summary>
public static class DaprExtensions
{
    /// <summary>
    /// 配置 Dapr CAP 中间件，映射订阅端点
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
