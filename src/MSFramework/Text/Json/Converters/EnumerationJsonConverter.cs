using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumerationJsonConverter<T> : JsonConverter<T> where T : Enumeration
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
        return Enumeration.Parse(typeToConvert, value) as T;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Id);
    }
}
