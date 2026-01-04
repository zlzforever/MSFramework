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

/// <summary>
///
/// </summary>
/// <param name="groupId"></param>
/// <param name="daprOptions"></param>
/// <param name="configureApi"></param>
/// <param name="logger"></param>
public class DaprConsumerClient(
    string groupId,
    IOptionsMonitor<DaprOptions> daprOptions,
    Action<string, string, string, Delegate> configureApi,
    ILogger<DaprConsumerClient> logger)
    : IConsumerClient
{
    private readonly DaprOptions _daprOptions = daprOptions.CurrentValue;

    /// <summary>
    ///
    /// </summary>
    /// <param name="topics"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task SubscribeAsync(IEnumerable<string> topics)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task ListeningAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task CommitAsync(object sender)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task RejectAsync(object sender)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    public BrokerAddress BrokerAddress => new("Dapr", null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="topics"></param>
    /// <exception cref="ArgumentNullException"></exception>
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
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

    /// <summary>
    ///
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    ///
    /// </summary>
    public Func<TransportMessage, object, Task> OnMessageCallback { get; set; }

    /// <summary>
    ///
    /// </summary>
    public Action<LogMessageEventArgs> OnLogCallback { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
