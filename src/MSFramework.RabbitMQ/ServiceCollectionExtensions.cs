using System;
using MicroserviceFramework.EventBus;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace MicroserviceFramework.RabbitMQ
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseRabbitMqEventBus(this MicroserviceFrameworkBuilder builder,
			Action<RabbitMqOptions> configure = null)
		{
			var options = new RabbitMqOptions();
			configure?.Invoke(options);

			builder.Services.AddSingleton(options);
			builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
			builder.Services.AddSingleton<IConnectionFactory>(provider =>
			{
				var opts = provider.GetRequiredService<RabbitMqOptions>();
				var connectionFactory = new ConnectionFactory
				{
					HostName = opts.HostName,
					DispatchConsumersAsync = true
				};
				if (options.Port > 0)
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
			return builder;
		}
	}
}