using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EventBus
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventBus(this IServiceCollection services,
			Action<EventBusBuilder> builderAction = null)
		{
			var builder = new EventBusBuilder(services);
			builderAction?.Invoke(builder);

			builder.Services.AddSingleton<IEventBusSubscriptionStore, InMemoryEventBusSubscriptionStore>();
			builder.Services.AddSingleton<IEventBus, PassThroughEventBus>();

			return services;
		}
	}
}