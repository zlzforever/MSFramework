using MicroserviceFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace MicroserviceFramework.Newtonsoft
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static MicroserviceFrameworkBuilder UseNewtonsoftSerializer(this MicroserviceFrameworkBuilder builder)
		{
			builder.Services.TryAddSingleton(new JsonSerializerSettings());
			builder.Services.AddSingleton<ISerializer, NewtonsoftSerializer>();

			return builder;
		}
	}
}