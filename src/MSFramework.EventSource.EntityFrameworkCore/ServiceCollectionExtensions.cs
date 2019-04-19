using Microsoft.Extensions.DependencyInjection;
using MSFramework.EventBus;

namespace MSFramework.EventSource.EntityFrameworkCore
{
	public static class ServiceCollectionExtensions
	{
		public static EventBusBuilder UseEfEventSource(this EventBusBuilder builder)
		{
			builder.Services.AddScoped<IEventSourceService, EventSourceService>();
			return builder;
		}
	}
}