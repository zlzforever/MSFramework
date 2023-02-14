using Microsoft.AspNetCore.Builder;

namespace DotNetCore.CAP.Dapr;

public static class DaprExtensions
{
    public static void UseDaprCap(this WebApplication app)
    {
        DaprConsumerClientFactory.MapEndpointRoute = (path, pubsub, topic, @delegate) =>
        {
            app.MapPost(path, @delegate).WithTopic(pubsub, topic);
        };
    }
}
