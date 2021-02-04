using System;
using MicroserviceFramework.Shared;
using Newtonsoft.Json;

namespace MicroserviceFramework.Newtonsoft.Converters
{
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
			if (reader.TokenType != JsonToken.String)
			{
				throw new JsonSerializationException($"Expected String but got {reader.TokenType}.");
			}

			return new ObjectId((string) reader.Value);
		}
	}
}