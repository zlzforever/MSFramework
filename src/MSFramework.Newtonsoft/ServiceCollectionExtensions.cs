using System;
using MicroserviceFramework.Serializer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace MicroserviceFramework.Newtonsoft
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseNewtonsoftJson(this MicroserviceFrameworkBuilder builder,
			Action<JsonSerializerSettings> configure = null)
		{
			var settings = new JsonSerializerSettings();
			configure?.Invoke(settings);

			builder.Services.TryAddSingleton(settings);
			builder.Services.TryAddSingleton<ISerializer, NewtonsoftSerializer>();
			return builder;
		}
	}
}