using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EventBus
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddLocalEventBus(this IServiceCollection services)
		{
			services.AddSingleton<IEventBusSubscriptionStore, InMemoryEventBusSubscriptionStore>();
			services.AddSingleton<IEventBus, PassThroughEventBus>();

			return services;
		}
	}
}