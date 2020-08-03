using System;
using MSFramework.Common;
using Newtonsoft.Json;

namespace MSFramework.AspNetCore.Infrastructure
{
	public class ObjectIdConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
			{
				throw new JsonSerializationException($"Expected String but got {reader.TokenType}.");
			}

			return new ObjectId((string) reader.Value);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ObjectId);
		}
	}
}