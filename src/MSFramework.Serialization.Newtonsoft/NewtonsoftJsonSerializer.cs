using System;
using System.IO;
using System.Text;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///
/// </summary>
public class NewtonsoftJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerSettings _settings;

    /// <summary>
    ///
    /// </summary>
    /// <param name="settings"></param>
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, _settings);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(Stream json)
    {
        using var reader = new StreamReader(json);
        return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), _settings);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public object Deserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type, _settings);
    }
}
