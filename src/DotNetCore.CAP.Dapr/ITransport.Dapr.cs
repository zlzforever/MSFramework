using System.Text;
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
    private readonly CapOptions _capOptions;

    public DaprTransport(IOptionsMonitor<DaprOptions> daprOptions,
        DaprClient daprClient, ILogger<DaprTransport> logger, IOptions<CapOptions> capOptions)
    {
        _daprClient = daprClient;
        _logger = logger;
        _capOptions = capOptions.Value;
        _daprOptions = daprOptions.CurrentValue;
    }

    public BrokerAddress BrokerAddress => new("Dapr", _daprOptions.GrpcEndpoint);

    public async Task<OperateResult> SendAsync(TransportMessage message)
    {
        try
        {
            var topicName = message.GetName();
            var group = message.GetGroup();
            if (group == null)
            {
                message.Headers.Add(Headers.Group, $"{_capOptions.DefaultGroupName}.{_capOptions.Version}");
            }

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
