using System.Text.Json;
using MicroserviceFramework.Serialization.Converters;

namespace MicroserviceFramework.Serialization
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static MicroserviceFrameworkBuilder UseDefaultSerializer(this MicroserviceFrameworkBuilder builder,
			JsonSerializerOptions options = null)
		{
			if (options == null)
			{
				options = new JsonSerializerOptions();
				options.Converters.Add(new ObjectIdJsonConverter());
				options.Converters.Add(new EnumerationJsonConverterFactory());
				options.Converters.Add(new EnumerationJsonConverter());
				options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			}

			Default.Serializer = new DefaultSerializer(options);
			return builder;
		}
	}
}