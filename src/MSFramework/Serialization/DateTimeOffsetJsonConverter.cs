using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroserviceFramework.Serialization;

public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetInt32(out var v) ? DateTimeOffset.FromUnixTimeSeconds(v) : DateTimeOffset.UnixEpoch;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
