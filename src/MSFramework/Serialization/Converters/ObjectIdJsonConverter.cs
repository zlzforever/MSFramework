using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Serialization.Converters
{
	public class ObjectIdJsonConverter : JsonConverter<ObjectId>
	{
		public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			System.Diagnostics.Debug.Assert(typeToConvert == typeof(ObjectId));

			return new ObjectId(reader.ValueSpan);
		}

		public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}