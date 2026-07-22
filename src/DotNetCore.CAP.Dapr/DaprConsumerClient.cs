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
///     CAP 的 Dapr 消费者客户端，通过 Dapr Pub/Sub API 订阅和处理消息
/// </summary>
/// <param name="groupId">消费者组标识</param>
/// <param name="daprOptions">Dapr 配置选项</param>
/// <param name="configureApi">配置 API 路由的回调</param>
/// <param name="logger">日志记录器</param>
public class DaprConsumerClient(
    string groupId,
    IOptionsMonitor<DaprOptions> daprOptions,
    Action<string, string, string, Delegate> configureApi,
    ILogger<DaprConsumerClient> logger)
    : IConsumerClient
{
    private readonly DaprOptions _daprOptions = daprOptions.CurrentValue;

    /// <summary>
    ///     异步订阅主题（暂未实现）
    /// </summary>
    /// <param name="topics">主题列表</param>
    /// <returns>异步任务</returns>
    /// <exception cref="NotImplementedException">该方法尚未实现</exception>
    public Task SubscribeAsync(IEnumerable<string> topics)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     异步监听消息（暂未实现）
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    /// <exception cref="NotImplementedException">该方法尚未实现</exception>
    public Task ListeningAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     异步提交消息（暂未实现）
    /// </summary>
    /// <param name="sender">发送方对象</param>
    /// <returns>异步任务</returns>
    /// <exception cref="NotImplementedException">该方法尚未实现</exception>
    public Task CommitAsync(object sender)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     异步拒绝消息（暂未实现）
    /// </summary>
    /// <param name="sender">发送方对象</param>
    /// <returns>异步任务</returns>
    /// <exception cref="NotImplementedException">该方法尚未实现</exception>
    public Task RejectAsync(object sender)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Dapr 消息代理地址信息
    /// </summary>
    public BrokerAddress BrokerAddress => new("Dapr", null);

    /// <summary>
    ///     订阅主题列表，为每个主题注册 Dapr Pub/Sub API 路由
    /// </summary>
    /// <param name="topics">主题列表</param>
    /// <exception cref="ArgumentNullException">topics 为 null 时抛出</exception>
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
    ///     开始监听消息（同步空实现，消息由 Dapr 通过 HTTP 回调驱动）
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
    {
    }

    /// <summary>
    ///     提交消息（无需额外操作，Dapr 自动管理 ACK）
    /// </summary>
    /// <param name="sender">发送方对象</param>
    public void Commit(object sender)
    {
    }

    /// <summary>
    ///     拒绝消息，设置 HTTP 400 状态码触发 Dapr 重新投递
    /// </summary>
    /// <param name="sender">发送方对象，应为 IHttpContextAccessor</param>
    /// <exception cref="ApplicationException">sender 不是 IHttpContextAccessor 或无 HttpContext 时抛出</exception>
    public void Reject(object sender)
    {
        if (sender is IHttpContextAccessor httpContextAccessor && httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Response.StatusCode = 400;
        }
    }

    /// <summary>
    ///     释放资源（无需操作）
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    ///     收到消息时的回调委托
    /// </summary>
    public Func<TransportMessage, object, Task> OnMessageCallback { get; set; }

    /// <summary>
    ///     日志事件回调委托
    /// </summary>
    public Action<LogMessageEventArgs> OnLogCallback { get; set; }

    /// <summary>
    ///     异步释放资源（暂未实现）
    /// </summary>
    /// <returns>异步值任务</returns>
    /// <exception cref="NotImplementedException">该方法尚未实现</exception>
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
