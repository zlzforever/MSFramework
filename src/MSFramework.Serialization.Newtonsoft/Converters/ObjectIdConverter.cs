using System;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MicroserviceFramework.Serialization.Newtonsoft.Converters;

public class ObjectIdConverter : JsonConverter<ObjectId>
{
    public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

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
