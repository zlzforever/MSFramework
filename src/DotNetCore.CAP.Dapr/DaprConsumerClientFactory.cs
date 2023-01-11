using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

public class DaprConsumerClientFactory : IConsumerClientFactory
{
    private readonly IOptionsMonitor<DaprOptions> _daprOptions;

    public DaprConsumerClientFactory(IOptionsMonitor<DaprOptions> daprOptions)
    {
        _daprOptions = daprOptions;
    }

    public IConsumerClient Create(string groupId)
    {
        try
        {
            return new DaprConsumerClient(groupId, _daprOptions);
        }
        catch (Exception e)
        {
            throw new BrokerConnectionException(e);
        }
    }
}
