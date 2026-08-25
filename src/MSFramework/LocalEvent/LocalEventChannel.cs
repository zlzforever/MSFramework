using System.Threading.Channels;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 单个服务容器使用的本地事件通道。
/// </summary>
internal sealed class LocalEventChannel
{
    public Channel<(SessionSnapshot Session, EventBase EventData)> EventChannel { get; } =
        Channel.CreateBounded<(SessionSnapshot Session, EventBase EventData)>(
            new BoundedChannelOptions(2000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true
            });
}
