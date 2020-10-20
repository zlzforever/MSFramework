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
			serviceCollection.TryAddSingleton<Initializer.InitializerBase, EventHandlerRegister>();
			return serviceCollection;
		}
		
		public static MicroserviceFrameworkBuilder UseEventBus(this MicroserviceFrameworkBuilder builder)
		{
			builder.Services.AddEventBus();
			return builder;
		}

	}
}