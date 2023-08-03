using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.EventBus;

public static class ServiceCollectionExtensions
{
    public static MicroserviceFrameworkBuilder UseEventBus(this MicroserviceFrameworkBuilder builder,
        Action<IEventBusSubscriber, EventBusOptions> configure = null)
    {
        var options = new EventBusOptions();
        IEventBusSubscriber subscriber = new DependencyInjectionEventBusSubscriber(builder.Services);

        configure?.Invoke(subscriber, options);

        builder.Services.TryAddSingleton(options);
        builder.Services.TryAddSingleton<IEventPublisher, LocalEventPublisher>();

        MicroserviceFrameworkLoaderContext.Get(builder.Services).ResolveType += type =>
        {
            subscriber.Subscribe(type);
        };
        return builder;
    }
}
