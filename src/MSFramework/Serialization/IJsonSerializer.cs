using System;
using System.IO;

namespace MicroserviceFramework.Serialization;

public interface IJsonSerializer
{
    string Serialize(object obj);
    byte[] SerializeToUtf8Bytes(object obj);
    T Deserialize<T>(string json);
    T Deserialize<T>(Stream json);
    object Deserialize(string json, Type type);
}
