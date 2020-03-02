using System;
using EventBus.RabbitMQ.DependencyInjection;

namespace MSFramework.EventBus.RabbitMQ
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddRabbitMQEventBus(this MSFrameworkBuilder builder,
			params Type[] handlerTypes)
		{
			builder.Services.AddRabbitMQEventBus(handlerTypes);
			return builder;
		}
	}
}