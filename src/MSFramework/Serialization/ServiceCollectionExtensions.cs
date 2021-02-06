using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Serialization
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddSerializer(this IServiceCollection serviceCollection,
			Action<JsonSerializerOptions> configure = null)
		{
			var options = new JsonSerializerOptions();
			configure?.Invoke(options);
			serviceCollection.TryAddSingleton(options);
			serviceCollection.AddSingleton<ISerializer, DefaultSerializer>();
			return serviceCollection;
		}
	}
}