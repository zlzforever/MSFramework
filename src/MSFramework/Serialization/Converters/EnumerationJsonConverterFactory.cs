using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Serialization.Converters
{
	public class EnumerationJsonConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(Enumeration).IsAssignableFrom(typeToConvert);
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			return new EnumerationJsonConverter();
		}
	}
}