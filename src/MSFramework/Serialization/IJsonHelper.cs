using System;

namespace MicroserviceFramework.Serialization;

public interface IJsonHelper
{
    string Serialize(object obj);
    T Deserialize<T>(string json);
    object Deserialize(string json, Type type);
}
