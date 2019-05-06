using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EventBus
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseLocalEventBus(this MSFrameworkBuilder builder,
			Action<EventBusBuilder> configure = null)
		{
			EventBusBuilder eBuilder = new EventBusBuilder(builder.Services);
			configure?.Invoke(eBuilder);

			builder.Services.AddLocalEventBus();

			return builder;
		}

		public static IServiceCollection AddLocalEventBus(this IServiceCollection services)
		{			
			services.AddSingleton<IEventBus, PassThroughEventBus>();
			return services;
		}
	}
}