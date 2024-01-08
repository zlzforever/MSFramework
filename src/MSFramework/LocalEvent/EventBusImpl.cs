using System.Threading.Channels;
using MicroserviceFramework.Application;

namespace MicroserviceFramework.LocalEvent;

internal static class EventBusImpl
{
    internal static readonly Channel<(ISession Session, EventBase EventData)>
        EventChannel =
            Channel.CreateUnbounded<(ISession, EventBase)>();
}
