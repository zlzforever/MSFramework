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

internal class DaprTransport(
    IOptionsMonitor<DaprOptions> daprOptions,
    DaprClient daprClient,
    ILogger<DaprTransport> logger)
    : ITransport
{
    private readonly DaprOptions _daprOptions = daprOptions.CurrentValue;
    private readonly ILogger _logger = logger;

    public BrokerAddress BrokerAddress => new("Dapr", null);

    public async Task<OperateResult> SendAsync(TransportMessage message)
    {
        try
        {
            var topicName = message.GetName();
            var body = Encoding.UTF8.GetString(message.Body.Span);

            await daprClient.PublishEventAsync(_daprOptions.Pubsub, topicName,
                new DaprTransportMessage { Headers = message.Headers, Body = body });

            _logger.LogDebug($"dapr topic message [{message.GetName()}] has been published.");

            // 只要能执行成功， 必定是事件发送成功了
            return OperateResult.Success;
        }
        catch (Exception ex)
        {
            var wrapperEx = new PublisherSentFailedException(ex.Message, ex);

            return OperateResult.Failed(wrapperEx);
        }
    }
}
