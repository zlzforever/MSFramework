using System;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MicroserviceFramework.Serialization.Newtonsoft.Converters;

/// <summary>
///
/// </summary>
public class ObjectIdConverter : JsonConverter<ObjectId>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    /// <exception cref="JsonSerializationException"></exception>
    public override ObjectId ReadJson(JsonReader reader, Type objectType, ObjectId existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return ObjectId.Empty;
        }

        if (reader.TokenType != JsonToken.String)
        {
            throw new JsonSerializationException($"Expected String but got {reader.TokenType}.");
        }

        if (reader.Value == null || (string)reader.Value == string.Empty)
        {
            return ObjectId.Empty;
        }

        return new ObjectId((string)reader.Value);
    }
}
