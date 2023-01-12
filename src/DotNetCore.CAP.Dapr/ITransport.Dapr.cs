using System;
using System.Text;
using System.Threading.Tasks;
using Dapr.Client;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

internal class DaprTransport : ITransport
{
    private readonly DaprOptions _daprOptions;
    private readonly DaprClient _daprClient;
    private readonly ILogger _logger;

    public DaprTransport(IOptionsMonitor<DaprOptions> daprOptions,
        DaprClient daprClient, ILogger<DaprTransport> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
        _daprOptions = daprOptions.CurrentValue;
    }

    public BrokerAddress BrokerAddress => new("Dapr", null);

    public async Task<OperateResult> SendAsync(TransportMessage message)
    {
        try
        {
            var topicName = message.GetName();
            var body = Encoding.UTF8.GetString(message.Body.Span);

            await _daprClient.PublishEventAsync(_daprOptions.Pubsub, topicName,
                new DaprTransportMessage { Headers = message.Headers, Body = body });

            _logger.LogDebug($"dapr topic message [{message.GetName()}] has been published.");

            // 只要能执行成功， 必定是事件发送成功了
            return OperateResult.Success;
        }
        catch (Exception ex)
        {
            var wapperEx = new PublisherSentFailedException(ex.Message, ex);

            return OperateResult.Failed(wapperEx);
        }
    }
}
