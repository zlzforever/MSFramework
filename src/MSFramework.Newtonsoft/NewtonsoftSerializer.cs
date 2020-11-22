using System;
using MicroserviceFramework.Serialization;
using Newtonsoft.Json;

namespace MicroserviceFramework.Newtonsoft
{
	public class NewtonsoftSerializer : ISerializer
	{
		private readonly JsonSerializerSettings _serializerSettings;

		public NewtonsoftSerializer(JsonSerializerSettings serializerSettings)
		{
			_serializerSettings = serializerSettings;
		}

		public string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, _serializerSettings);
		}

		public T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, _serializerSettings);
		}

		public object Deserialize(string json, Type type)
		{
			return JsonConvert.DeserializeObject(json, type, _serializerSettings);
		}
	}
}