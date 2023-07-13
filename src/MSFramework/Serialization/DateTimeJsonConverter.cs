using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.Serialization;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetInt32(out var v) ? DateTimeOffset.FromUnixTimeSeconds(v).LocalDateTime : DateTime.UnixEpoch;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
