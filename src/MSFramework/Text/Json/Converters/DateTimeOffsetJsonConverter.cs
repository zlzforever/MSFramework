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
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? DateTimeOffset.MinValue : DateTimeOffset.Parse(value);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.TryGetInt32(out var v) ? DateTimeOffset.FromUnixTimeSeconds(v) : DateTimeOffset.UnixEpoch;
        }

        throw new NotSupportedException("不支持的数据类型");
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
