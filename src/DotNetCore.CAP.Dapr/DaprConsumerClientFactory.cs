using System;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

public class DaprConsumerClientFactory : IConsumerClientFactory
{
    public static Action<string, string, string, Delegate> MapEndpointRoute;

    private readonly IOptionsMonitor<DaprOptions> _daprOptions;
    private readonly ILoggerFactory _loggerFactory;

    public DaprConsumerClientFactory(IOptionsMonitor<DaprOptions> daprOptions,
        ILoggerFactory loggerFactory)
    {
        _daprOptions = daprOptions;
        _loggerFactory = loggerFactory;
    }

    public IConsumerClient Create(string groupId)
    {
        try
        {
            if (MapEndpointRoute == null)
            {
                throw new ArgumentNullException(nameof(MapEndpointRoute));
            }

            var logger = _loggerFactory.CreateLogger<DaprConsumerClient>();
            return new DaprConsumerClient(groupId, _daprOptions, MapEndpointRoute, logger);
        }
        catch (Exception e)
        {
            throw new BrokerConnectionException(e);
        }
    }
}
