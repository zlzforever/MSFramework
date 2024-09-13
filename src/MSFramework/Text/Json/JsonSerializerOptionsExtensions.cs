using System.Text.Json;
using MicroserviceFramework.Text.Json.Converters;

namespace MicroserviceFramework.Text.Json;

/// <summary>
///
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    public static void AddDefaultConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new ObjectIdJsonConverter());
        options.Converters.Add(new EnumerationJsonConverterFactory());
        options.Converters.Add(new DateTimeJsonConverter());
        options.Converters.Add(new DateTimeOffsetJsonConverter());
    }
}
