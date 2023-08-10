using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MicroserviceFramework.Serialization.Newtonsoft;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseNewtonsoftJsonSerializer(this MicroserviceFrameworkBuilder builder,
        JsonSerializerSettings settings = null)
    {
        if (settings != null)
        {
            builder.Services.AddSingleton<IJsonSerializerFactory>(new DefaultJsonSerializerFactory(settings));
        }
        else
        {
            builder.Services.AddSingleton<IJsonSerializerFactory>(provider =>
                new DefaultJsonSerializerFactory(provider.GetRequiredService<JsonSerializerSettings>()));
        }

        return builder;
    }
}
