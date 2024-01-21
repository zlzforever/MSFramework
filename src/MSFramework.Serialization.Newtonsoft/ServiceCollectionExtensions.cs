using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            var injectSettings = new JsonSerializerSettings();
            injectSettings.Converters.Add(new ObjectIdConverter());
            injectSettings.Converters.Add(new EnumerationConverter());
            injectSettings.ContractResolver = new CompositeContractResolver
            {
                new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
            };
            builder.Services.TryAddSingleton(injectSettings);
            builder.Services.TryAddSingleton<IJsonSerializer>(new NewtonsoftJsonSerializer(injectSettings));
        }

        return builder;
    }
}
