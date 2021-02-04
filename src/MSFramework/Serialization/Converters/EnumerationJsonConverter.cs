using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Serialization.Converters
{

	public class EnumerationJsonConverter : JsonConverter<Enumeration>
	{
		public override Enumeration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
#if NETSTANDARD2_1
			var value = Encoding.UTF8.GetString(reader.ValueSpan);
#else
			var value = Encoding.UTF8.GetString(reader.ValueSpan.ToArray());
#endif

			return Enumeration.Parse(typeToConvert, value);
		}

		public override void Write(Utf8JsonWriter writer, Enumeration value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Id);
		}
	}
}