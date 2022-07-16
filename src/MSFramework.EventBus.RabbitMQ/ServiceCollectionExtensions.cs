using MicroserviceFramework.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MicroserviceFramework.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {
        public static MicroserviceFrameworkBuilder UseEventBusRabbitMQ(this MicroserviceFrameworkBuilder builder,
            IConfiguration configuration)
        {
            builder.RegisterEventHandlers();

            builder.Services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
            builder.Services.AddSingleton<PersistentConnection>();
            builder.Services.AddSingleton<IEventExecutor, DefaultEventExecutor>();
            builder.Services.AddSingleton<IConnectionFactory>(provider =>
            {
                var opts = provider.GetRequiredService<IOptionsMonitor<RabbitMQOptions>>().CurrentValue;
                var connectionFactory = new ConnectionFactory
                {
                    HostName = opts.HostName,
                    DispatchConsumersAsync = true
                };
                if (opts.Port > 0)
                {
                    connectionFactory.Port = opts.Port;
                }

                if (!string.IsNullOrWhiteSpace(opts.UserName))
                {
                    connectionFactory.UserName = opts.UserName;
                }

                if (!string.IsNullOrWhiteSpace(opts.Password))
                {
                    connectionFactory.Password = opts.Password;
                }

                return connectionFactory;
            });
            builder.Services.AddSingleton<IEventBus>((services) =>
            {
                var connection = services.GetRequiredService<PersistentConnection>();
                var eventExecutor = services.GetRequiredService<IEventExecutor>();
                var logger = services.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBus = new EventBusRabbitMQ(services.GetRequiredService<IOptionsMonitor<RabbitMQOptions>>(),
                    connection, eventExecutor, logger);
                eventBus.SubscribeAllEventTypes();
                return eventBus;
            });
            return builder;
        }
    }
}