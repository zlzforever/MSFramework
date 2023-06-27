using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace MicroserviceFramework.Text.Json.Converters;

public class ObjectIdJsonConverter : JsonConverter<ObjectId>
{
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        System.Diagnostics.Debug.Assert(typeToConvert == typeof(ObjectId));
        return reader.ValueSpan.Length != 0 ? new ObjectId(Encoding.UTF8.GetString(reader.ValueSpan)) : ObjectId.Empty;
    }

    public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
