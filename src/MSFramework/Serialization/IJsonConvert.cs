using System;

namespace MSFramework.Serialization
{
	public interface IJsonConvert
	{
		string SerializeObject(object value);

		object DeserializeObject(string value);

		object DeserializeObject(string value, Type type);

		T DeserializeObject<T>(string value);

		T DeserializeAnonymousType<T>(string value, T anonymousTypeObject);
	}
}