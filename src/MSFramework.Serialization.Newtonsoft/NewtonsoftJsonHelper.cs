using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MicroserviceFramework.Serialization.Newtonsoft;

public class NewtonsoftJsonHelper : IJsonHelper
{
    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
    }

    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public T Deserialize<T>(Stream json)
    {
        using var reader = new StreamReader(json);
        return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
    }

    public object Deserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type);
    }
}
