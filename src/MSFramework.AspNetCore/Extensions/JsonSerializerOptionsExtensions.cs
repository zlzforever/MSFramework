using System.Text.Json;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json.Converters;

namespace MicroserviceFramework.AspNetCore.Extensions;

public static class JsonSerializerOptionsExtensions
{
    public static void AddDefaultConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new ObjectIdJsonConverter());
        options.Converters.Add(new EnumerationJsonConverterFactory());
        options.Converters.Add(new PagedResultJsonConverterFactory());
    }
}
