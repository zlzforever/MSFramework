using System.Text.Json;
using MicroserviceFramework.Serialization.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Serialization
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static MicroserviceFrameworkBuilder UseDefaultSerializer(this MicroserviceFrameworkBuilder builder)
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ObjectIdJsonConverter());
			options.Converters.Add(new EnumerationJsonConverterFactory());
			options.Converters.Add(new EnumerationJsonConverter());
			options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

			builder.Services.TryAddSingleton(options);
			builder.Services.AddSingleton<ISerializer, DefaultSerializer>();

			return builder;
		}
	}
}