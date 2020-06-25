using System;
using System.Collections.Generic;
using System.Linq;
using EventBus.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSFramework.Domain.Event;
using MSFramework.RabbitMQ;

namespace MSFramework.EventBus.RabbitMQ
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseRabbitMQEventDispatcher(this MSFrameworkBuilder builder,
			RabbitMQOptions options, params Type[] eventTypes)
		{
			var excludeAssembly = typeof(MSFrameworkBuilder).Assembly;
			if (eventTypes.Any(x => x.Assembly != excludeAssembly))
			{
				var list = new List<Type>(eventTypes) {typeof(MSFrameworkBuilder)};
				builder.Services.AddEventDispatcher(list.ToArray());
			}
			else
			{
				builder.Services.AddEventDispatcher(eventTypes);
			}

			builder.Services.AddSingleton(options);
			builder.Services.AddScoped<IEventDispatcher, RabbitMQEventDispatcher>();
			return builder;
		}

		public static MSFrameworkBuilder UseRabbitMQEventDispatcher(this MSFrameworkBuilder builder,
			Action<RabbitMQOptions> configure, params Type[] eventTypes)
		{
			var options = new RabbitMQOptions();
			configure?.Invoke(options);

			builder.UseRabbitMQEventDispatcher(options, eventTypes);
			return builder;
		}
	}
}