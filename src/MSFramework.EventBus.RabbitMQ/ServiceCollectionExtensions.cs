using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MSFramework.EventBus.RabbitMQ
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseRabbitMQEventBus(this MSFrameworkBuilder builder)
		{
			builder.Services.AddRabbitMQEventBus();
			return builder;
		}

		public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services)
		{
			services.AddSingleton<RabbitMQOptions>();
			services.AddSingleton(provider =>
			{
				var options = provider.GetRequiredService<RabbitMQOptions>();

				var factory = new ConnectionFactory
				{
					HostName = options.HostName,
					Port = options.Port,
					DispatchConsumersAsync = true
				};

				if (!string.IsNullOrEmpty(options.UserName))
				{
					factory.UserName = options.UserName;
				}

				if (!string.IsNullOrEmpty(options.Password))
				{
					factory.Password = options.Password;
				}
				return new RabbitMQConnection(factory,
					provider.GetRequiredService<ILogger<RabbitMQConnection>>(),
					options.RetryCount);
			});
			services.AddSingleton<IEventBus, RabbitMQEventBus>();
			return services;
		}
	}
}