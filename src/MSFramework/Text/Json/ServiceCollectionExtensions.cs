using System.Linq;
using System.Text.Json;
using MicroserviceFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Text.Json;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseDefaultJsonSerializer(this MicroserviceFrameworkBuilder builder,
        JsonSerializerOptions options = null)
    {
        if (options != null)
        {
            builder.Services.AddSingleton<IJsonSerializerFactory>(new DefaultJsonSerializerFactory(options));
        }
        else
        {
            builder.Services.AddSingleton<IJsonSerializerFactory>(provider =>
                new DefaultJsonSerializerFactory(provider.GetRequiredService<JsonSerializerOptions>()));
        }

        return builder;
    }
}
