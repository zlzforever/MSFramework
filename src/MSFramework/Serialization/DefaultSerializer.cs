using System;
using System.Text.Json;

namespace MicroserviceFramework.Serialization
{
	public class DefaultSerializer : ISerializer
	{
		private readonly JsonSerializerOptions _options;

		public DefaultSerializer(JsonSerializerOptions options)
		{
			_options = options;
		}

		public string Serialize(object obj)
		{
			return JsonSerializer.Serialize(obj, _options);
		}

		public T Deserialize<T>(string json)
		{
			return JsonSerializer.Deserialize<T>(json, _options);
		}

		public object Deserialize(string json, Type returnType)
		{
			return JsonSerializer.Deserialize(json, returnType, _options);
		}
	}
}