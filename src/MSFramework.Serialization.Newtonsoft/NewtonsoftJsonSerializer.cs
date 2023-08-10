using System;
using System.IO;
using System.Text;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

public class NewtonsoftJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerSettings _settings;

    public NewtonsoftJsonSerializer(JsonSerializerSettings settings = null)
    {
        if (settings == null)
        {
            settings = new JsonSerializerSettings();
            settings.Converters.Add(new ObjectIdConverter());
            settings.Converters.Add(new EnumerationConverter());
            settings.ContractResolver = new CompositeContractResolver
            {
                new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
            };
        }

        _settings = settings;
    }

    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, _settings);
    }

    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings));
    }

    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }

    public T Deserialize<T>(Stream json)
    {
        using var reader = new StreamReader(json);
        return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), _settings);
    }

    public object Deserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type, _settings);
    }
}
