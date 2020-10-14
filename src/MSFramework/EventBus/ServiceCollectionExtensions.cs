using MicroserviceFramework.Initializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.EventBus
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventBus(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IEventBus, ChannelEventBus>();
			serviceCollection.TryAddSingleton<ISubscriptionInfoStore, SubscriptionInfoStore>();
			serviceCollection
				.TryAddSingleton<IEventHandlerFactory, DependencyInjectionEventHandlerFactory>();
			serviceCollection.TryAddSingleton<Initializer, EventBusInitializer>();
			return serviceCollection;
		}
	}
}