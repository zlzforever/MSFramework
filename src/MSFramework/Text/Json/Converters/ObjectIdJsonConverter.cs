using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace MicroserviceFramework.Text.Json.Converters
{
    public class ObjectIdJsonConverter : JsonConverter<ObjectId>
    {
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            System.Diagnostics.Debug.Assert(typeToConvert == typeof(ObjectId));
            var bytes = reader.ValueSpan.ToArray();
            // todo: 优化成直接读取
            var str = Encoding.UTF8.GetString(bytes);
            return reader.ValueSpan.Length != 0 ? new ObjectId(str) : ObjectId.Empty;
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}