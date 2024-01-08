using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.LocalEvent;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalEventPublisher(this IServiceCollection services)
    {
        services.TryAddScoped<IEventPublisher, LocalEventPublisher>();
        services.AddHostedService<EventConsumeService>();

        MicroserviceFrameworkLoaderContext.Get(services).ResolveType += type =>
        {
            var serviceTypes = type
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == Defaults.EventHandlerType);

            foreach (var serviceType in serviceTypes)
            {
                var eventType = serviceType.GetGenericArguments()[0];
                EventHandlerStore.Add(eventType, type);
                services.TryAddScoped(type);
            }
        };

        return services;
    }

    public static MicroserviceFrameworkBuilder UseLocalEventPublisher(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddLocalEventPublisher();
        return builder;
    }
}
