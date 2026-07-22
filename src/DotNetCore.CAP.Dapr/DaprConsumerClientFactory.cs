using System;
using System.Threading.Tasks;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.Dapr;

/// <summary>
/// Dapr 消费者客户端工厂，创建 Dapr 消息队列消费者
/// </summary>
/// <param name="daprOptions">Dapr 选项</param>
/// <param name="loggerFactory">日志工厂</param>
public class DaprConsumerClientFactory(
    IOptionsMonitor<DaprOptions> daprOptions,
    ILoggerFactory loggerFactory)
    : IConsumerClientFactory
{
    /// <summary>
    /// 映射 Dapr 订阅端点的静态委托
    /// </summary>
    public static Action<string, string, string, Delegate> MapEndpointRoute;

    /// <summary>
    /// 创建 Dapr 消费者客户端实例
    /// </summary>
    /// <param name="groupId">消费者组 ID</param>
    /// <returns>消费者客户端实例</returns>
    /// <exception cref="ArgumentNullException">MapEndpointRoute 未初始化</exception>
    /// <exception cref="BrokerConnectionException">创建消费者失败</exception>
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
    /// 异步创建 Dapr 消费者客户端（暂未实现）
    /// </summary>
    /// <param name="groupName">消费者组名称</param>
    /// <param name="groupConcurrent">组并发数</param>
    /// <returns>消费者客户端实例</returns>
    /// <exception cref="NotImplementedException">该方法暂未实现</exception>
    public Task<IConsumerClient> CreateAsync(string groupName, byte groupConcurrent)
    {
        throw new NotImplementedException();
    }
}
