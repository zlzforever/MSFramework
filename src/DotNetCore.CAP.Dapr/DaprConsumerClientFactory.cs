using System;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

public class DaprConsumerClientFactory(
    IOptionsMonitor<DaprOptions> daprOptions,
    ILoggerFactory loggerFactory)
    : IConsumerClientFactory
{
    public static Action<string, string, string, Delegate> MapEndpointRoute;

    public IConsumerClient Create(string groupId)
    {
        try
        {
            if (MapEndpointRoute == null)
            {
                throw new ArgumentNullException(nameof(MapEndpointRoute));
            }

            var logger = loggerFactory.CreateLogger<DaprConsumerClient>();
            return new DaprConsumerClient(groupId, daprOptions, MapEndpointRoute, logger);
        }
        catch (Exception e)
        {
            throw new BrokerConnectionException(e);
        }
    }
}
