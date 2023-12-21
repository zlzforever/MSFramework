using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            builder.Services.TryAddSingleton<IJsonSerializer>(new NewtonsoftJsonSerializer(settings));
        }
        else
        {
            builder.Services.TryAddSingleton<IJsonSerializer>(provider =>
            {
                var x = provider.GetService<JsonSerializerSettings>();
                return new NewtonsoftJsonSerializer(x);
            });
        }

        return builder;
    }
}
