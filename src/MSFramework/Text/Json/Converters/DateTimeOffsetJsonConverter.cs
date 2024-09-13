using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
///
/// </summary>
public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetInt32(out var v) ? DateTimeOffset.FromUnixTimeSeconds(v) : DateTimeOffset.UnixEpoch;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
