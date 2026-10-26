using System.Threading.Channels;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 单个服务容器使用的本地事件通道：有界（容量 2000）、按序（SingleReader）、背压（FullMode.Wait——
/// 写满时发布端阻塞等待）。通道仅存于进程内存：进程崩溃或 Host 停止时未消费的事件丢失，
/// 无重试/死信语义（见 <see cref="IEventPublisher"/> 的可靠性说明）。
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
