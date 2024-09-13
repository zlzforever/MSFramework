using System.Text.Json;
using MicroserviceFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Text.Json;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseTextJsonSerializer(this MicroserviceFrameworkBuilder builder,
        JsonSerializerOptions options = null)
    {
        if (options != null)
        {
            builder.Services.TryAddSingleton<IJsonSerializer>(new TextJsonSerializer(options));
        }
        else
        {
            builder.Services.TryAddSingleton(provider =>
            {
                var x = provider.GetService<JsonSerializerOptions>();
                return x != null ? new TextJsonSerializer(x) : TextJsonSerializer.Create();
            });
        }

        return builder;
    }
}
