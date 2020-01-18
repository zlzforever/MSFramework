using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EventBus
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddPassThroughEventBus(this MSFrameworkBuilder builder,
			Action<EventBusBuilder> configure = null)
		{
			var eBuilder = new EventBusBuilder(builder.Services);
			configure?.Invoke(eBuilder);

			builder.Services.AddPassThroughEventBus();

			return builder;
		}

		public static IServiceCollection AddPassThroughEventBus(this IServiceCollection services)
		{			
			services.AddScoped<IEventBus, PassThroughEventBus>();
			return services;
		}
	}
}