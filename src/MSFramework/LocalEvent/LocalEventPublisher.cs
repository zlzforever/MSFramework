using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.LocalEvent;

internal class LocalEventPublisher(
    IServiceProvider serviceProvider)
    : IEventPublisher
{
    internal static readonly Channel<(ISession Session, EventBase EventData)>
        EventChannel = Channel.CreateUnbounded<(ISession, EventBase)>();

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
    {
        Check.NotNull(@event, nameof(@event));
        var session = serviceProvider.GetService<ISession>();
        await EventChannel.Writer.WriteAsync((session, @event));
    }
}
