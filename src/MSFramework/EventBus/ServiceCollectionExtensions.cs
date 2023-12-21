using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.EventBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalEventPublisher(this IServiceCollection services)
    {
        services.TryAddScoped<IEventPublisher, LocalEventPublisher>();

        MicroserviceFrameworkLoaderContext.Get(services).ResolveType += type =>
        {
            var serviceTypes = type
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == Defaults.EventHandlerType);

            foreach (var serviceType in serviceTypes)
            {
                var eventType = serviceType.GetGenericArguments()[0];
                var descriptor =
                    new ServiceDescriptor(Defaults.EventHandlerType.MakeGenericType(eventType), type,
                        ServiceLifetime.Scoped);
                services.TryAddEnumerable(descriptor);
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
