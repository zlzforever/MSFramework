using System;
using Newtonsoft.Json;

namespace MSFramework.Serialization
{
	/// <summary>
	/// TODO: implement Options
	/// </summary>
	public class NewtonsoftJsonConvert : IJsonConvert
	{
		private readonly JsonConvertOptions _options;

		public NewtonsoftJsonConvert(JsonConvertOptions options)
		{
			_options = options;
		}

		public string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value);
		}

		public object DeserializeObject(string value)
		{
			return JsonConvert.DeserializeObject(value);
		}

		public object DeserializeObject(string value, Type type)
		{
			return JsonConvert.DeserializeObject(value, type);
		}

		public T DeserializeObject<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value);
		}

		public T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
		{
			return JsonConvert.DeserializeAnonymousType(value, anonymousTypeObject);
		}
	}
}