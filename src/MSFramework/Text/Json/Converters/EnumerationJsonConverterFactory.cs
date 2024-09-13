using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
///
/// </summary>
public class EnumerationJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Enumeration).IsAssignableFrom(typeToConvert);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var type = typeof(EnumerationJsonConverter<>).MakeGenericType(typeToConvert);
        return Activator.CreateInstance(type) as JsonConverter;
    }
}
