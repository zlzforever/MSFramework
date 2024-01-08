using System;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.LocalEvent;

internal class LocalEventPublisher(
    IServiceProvider serviceProvider)
    : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
    {
        Check.NotNull(@event, nameof(@event));
        var session = serviceProvider.GetService<ISession>();
        await EventBusImpl.EventChannel.Writer.WriteAsync((session, @event));
    }
}
