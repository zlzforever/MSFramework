using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Text.Json.Converters
{
    public class EnumerationJsonConverter<T> : JsonConverter<T> where T : Enumeration
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = Encoding.UTF8.GetString(reader.ValueSpan);
            return Enumeration.Parse(typeToConvert, value) as T;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Id);
        }
    }
}