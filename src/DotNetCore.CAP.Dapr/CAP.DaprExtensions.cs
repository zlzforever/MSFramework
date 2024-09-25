using Microsoft.AspNetCore.Builder;

namespace DotNetCore.CAP.Dapr;

/// <summary>
///
/// </summary>
public static class DaprExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="app"></param>
    public static void UseDaprCap(this WebApplication app)
    {
        DaprConsumerClientFactory.MapEndpointRoute = (path, pubsub, topic, @delegate) =>
        {
            app.MapPost(path, @delegate).WithTopic(pubsub, topic);
        };
    }
}
