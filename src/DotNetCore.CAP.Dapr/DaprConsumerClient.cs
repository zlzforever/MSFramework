using System.Text;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

public class DaprConsumerClient : IConsumerClient
{
    private readonly DaprOptions _daprOptions;
    private readonly string _daprPubsub;

    public DaprConsumerClient(string groupId, IOptionsMonitor<DaprOptions> daprOptions)
    {
        _daprPubsub = groupId.Replace(".v1", string.Empty);
        _daprOptions = daprOptions.CurrentValue;
    }

    public void Dispose()
    {
    }

    public BrokerAddress BrokerAddress => new("Dapr", _daprOptions.GrpcEndpoint);

    public void Subscribe(IEnumerable<string> topics)
    {
        if (topics == null)
        {
            throw new ArgumentNullException(nameof(topics));
        }

        foreach (var topic in topics)
        {
            var path = $"v1.0/publish/{_daprOptions.Pubsub}/{topic.Replace(".", "_")}";
            DaprExtensions.Web.MapPost(path,
                    ([FromBody] DaprTransportMessage message,
                        [FromServices] IHttpContextAccessor httpContextAccessor) =>
                    {
                        OnMessageCallback!(
                            new TransportMessage(message.Headers, Encoding.UTF8.GetBytes(message.Body)),
                            httpContextAccessor);
                    })
                .WithTopic(_daprOptions.Pubsub, topic);
            Console.WriteLine($"Subscribe: {path} -> {_daprPubsub}-{topic}");
        }
    }

    public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
    {
    }

    /// <summary>
    /// 不需要额外操作
    /// </summary>
    /// <param name="sender"></param>
    public void Commit(object? sender)
    {
    }

    /// <summary>
    /// MessageProcessor 抛出异常， 导致 Dapr API 访问 400， Dapr 会认为事件消费失败。产生 Dapr 级别的重试（重新投递)
    /// </summary>
    /// <param name="sender"></param>
    /// <exception cref="ApplicationException"></exception>
    public void Reject(object? sender)
    {
        if (sender is IHttpContextAccessor httpContextAccessor && httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Response.StatusCode = 400;
        }
    }

    public Func<TransportMessage, object?, Task>? OnMessageCallback { get; set; }
    public Action<LogMessageEventArgs>? OnLogCallback { get; set; }
}
