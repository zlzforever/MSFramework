using System;
using System.IO;
using System.Text.Json;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json.Converters;

namespace MicroserviceFramework.Text.Json;

public class JsonHelper : IJsonHelper
{
    private readonly JsonSerializerOptions _options;

    public static IJsonHelper Create()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdJsonConverter());
        options.Converters.Add(new EnumerationJsonConverterFactory());
        // options.Converters.Add(new EnumerationJsonConverter());
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = false;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        return new JsonHelper(options);
    }

    public JsonHelper(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, _options);
    }

    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public T Deserialize<T>(Stream json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public object Deserialize(string json, Type returnType)
    {
        return JsonSerializer.Deserialize(json, returnType, _options);
    }
}
