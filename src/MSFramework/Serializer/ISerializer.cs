using System;

namespace MicroserviceFramework.Serializer
{
	public interface ISerializer
	{
		string Serialize(object obj);
		T Deserialize<T>(string json);
		object Deserialize(string json, Type type);
	}
}