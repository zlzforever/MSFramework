using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
///
/// </summary>
public class ObjectIdJsonConverter : JsonConverter<ObjectId>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        System.Diagnostics.Debug.Assert(typeToConvert == typeof(ObjectId));
        return reader.ValueSpan.Length != 0 ? new ObjectId(System.Text.Encoding.UTF8.GetString(reader.ValueSpan)) : ObjectId.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
