using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EventBus
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddLocalEventBus(this MSFrameworkBuilder builder,
			Action<EventBusBuilder> configure = null)
		{
			EventBusBuilder eBuilder = new EventBusBuilder(builder.Services);
			configure?.Invoke(eBuilder);
			builder.Services.AddSingleton<IEventBusSubscriptionStore, InMemoryEventBusSubscriptionStore>();
			builder.Services.AddSingleton<IEventBus, PassThroughEventBus>();

			return builder;
		}
	}
}