using System;
using System.Threading.Tasks;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

/// <summary>
///
/// </summary>
/// <param name="daprOptions"></param>
/// <param name="loggerFactory"></param>
public class DaprConsumerClientFactory(
    IOptionsMonitor<DaprOptions> daprOptions,
    ILoggerFactory loggerFactory)
    : IConsumerClientFactory
{
    /// <summary>
    ///
    /// </summary>
    public static Action<string, string, string, Delegate> MapEndpointRoute;

    /// <summary>
    ///
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="BrokerConnectionException"></exception>
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="groupConcurrent"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IConsumerClient> CreateAsync(string groupName, byte groupConcurrent)
    {
        throw new NotImplementedException();
    }
}
