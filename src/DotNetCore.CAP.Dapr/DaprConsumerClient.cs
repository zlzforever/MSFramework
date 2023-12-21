using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

public class DaprConsumerClient(
    string groupId,
    IOptionsMonitor<DaprOptions> daprOptions,
    Action<string, string, string, Delegate> configureApi,
    ILogger<DaprConsumerClient> logger)
    : IConsumerClient
{
    private readonly DaprOptions _daprOptions = daprOptions.CurrentValue;

    public BrokerAddress BrokerAddress => new("Dapr", null);

    public void Subscribe(IEnumerable<string> topics)
    {
        if (topics == null)
        {
            throw new ArgumentNullException(nameof(topics));
        }

        foreach (var topic in topics)
        {
            var path = $"v1.0/publish/{_daprOptions.Pubsub}/{topic.Replace(".", "_")}";
            configureApi(path, _daprOptions.Pubsub, topic, ([FromBody] DaprTransportMessage message,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                message.Headers.Add(Headers.Group, groupId);

                OnMessageCallback!(
                    new TransportMessage(message.Headers, Encoding.UTF8.GetBytes(message.Body)),
                    httpContextAccessor);
            });

            logger.LogInformation($"Subscribe groupId {groupId}, topic {topic} on web api route {path}");
        }
    }

    public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
    {
    }

    /// <summary>
    /// 不需要额外操作
    /// </summary>
    /// <param name="sender"></param>
    public void Commit(object sender)
    {
    }

    /// <summary>
    /// MessageProcessor 抛出异常， 导致 Dapr API 返回 400， Dapr 会认为事件消费失败， 于是 Dapr 会重新投递消息（调用 API)
    /// </summary>
    /// <param name="sender"></param>
    /// <exception cref="ApplicationException"></exception>
    public void Reject(object sender)
    {
        if (sender is IHttpContextAccessor httpContextAccessor && httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Response.StatusCode = 400;
        }
    }

    public void Dispose()
    {
    }

    public Func<TransportMessage, object, Task> OnMessageCallback { get; set; }
    public Action<LogMessageEventArgs> OnLogCallback { get; set; }
}
